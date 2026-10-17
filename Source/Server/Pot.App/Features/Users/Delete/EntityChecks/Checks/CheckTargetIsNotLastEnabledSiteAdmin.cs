using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Users.Delete.Models;
using Pot.Data.Repositories.Users;

namespace Pot.App.Features.Users.Delete.EntityChecks.Checks;

/// <summary>
/// Rejects a deletion that would leave the target's site without an enabled administrator.
/// </summary>
/// <remarks>
/// Runs after <see cref="CheckTargetIsNotPlatformAdmin" />, so a target refused by both rules reports the
/// platform-administrator refusal, which has no in-app remedy.
/// </remarks>
internal sealed class CheckTargetIsNotLastEnabledSiteAdmin : UserDeleteCheckBase
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger _logger;

    public CheckTargetIsNotLastEnabledSiteAdmin(IUserRepository userRepository, ILogger<CheckTargetIsNotLastEnabledSiteAdmin> logger)
    {
        _userRepository = userRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    public override int Order => 3;

    /// <summary>
    /// Fails when the target is the site's only enabled administrator, otherwise delegates to the next check.
    /// </summary>
    /// <param name="state">The target being deleted.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The failing validation error, or <see langword="null" /> when the rule is satisfied.</returns>
    public override async Task<ApiDetailError?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        if (await IsOnlyEnabledSiteAdminAsync(state, cancellationToken))
        {
            return ApiDetailErrorFactory.CreateBadRequestError(
                nameof(Input.RowId),
                state.Target.RowId,
                "This is the site's only enabled administrator. Promote or enable another administrator first.");
        }

        return await base.HandleAsync(state, cancellationToken);
    }

    private async Task<bool> IsOnlyEnabledSiteAdminAsync(InputState state, CancellationToken cancellationToken)
    {
        var enabledAdmins = await _userRepository.GetEnabledAdminsForCurrentSiteAsync(cancellationToken);

        // Exactly one enabled administrator, and it is the row being deleted. A target that is not in the
        // population, or a population larger than one, leaves the site administrable.
        return enabledAdmins.Count == 1 && enabledAdmins[0].RowId == state.Target.RowId;
    }
}
