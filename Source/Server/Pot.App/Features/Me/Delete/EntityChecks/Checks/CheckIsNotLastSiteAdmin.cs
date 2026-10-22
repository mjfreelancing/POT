using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Me.Delete.Models;
using Pot.Data.Repositories.Users;

namespace Pot.App.Features.Me.Delete.EntityChecks.Checks;

/// <summary>
/// Rejects an account deletion when the caller is their site's only enabled administrator.
/// </summary>
/// <remarks>
/// The remedy names the site deletion action, because a caller who is their site's only user cannot promote
/// anyone else.
/// </remarks>
internal sealed class CheckIsNotLastSiteAdmin : AccountDeleteCheckBase
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger _logger;

    public CheckIsNotLastSiteAdmin(IUserRepository userRepository, ILogger<CheckIsNotLastSiteAdmin> logger)
    {
        _userRepository = userRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    public override int Order => 3;

    /// <summary>
    /// Fails when the caller is the site's only enabled administrator, otherwise delegates to the next check.
    /// </summary>
    /// <param name="state">The caller being deleted.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The failing validation error, or <see langword="null" /> when the rule is satisfied.</returns>
    public override async Task<ApiDetailError?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        if (await IsOnlyEnabledSiteAdminAsync(state, cancellationToken))
        {
            return ApiDetailErrorFactory.CreateBadRequestError(
                nameof(Input.Username),
                "You are your site's only enabled administrator. Promote or enable another administrator first, or delete the site instead.");
        }

        return await base.HandleAsync(state, cancellationToken);
    }

    private async Task<bool> IsOnlyEnabledSiteAdminAsync(InputState state, CancellationToken cancellationToken)
    {
        var enabledAdmins = await _userRepository.GetEnabledAdminsForCurrentSiteAsync(cancellationToken);

        return enabledAdmins.Count == 1 && enabledAdmins[0].RowId == state.Caller.RowId;
    }
}
