using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Users.Delete.Models;
using Pot.Data.Repositories.Users;

namespace Pot.App.Features.Users.Delete.EntityChecks.Checks;

/// <summary>
/// Rejects a deletion that would remove the last enabled platform administrator.
/// </summary>
/// <remarks>
/// A configured platform administrator can be deleted only while another enabled one remains: platform
/// administration comes from deployment configuration, so the platform would otherwise have no operator able to
/// change it.
/// </remarks>
internal sealed class CheckTargetIsNotPlatformAdmin : UserDeleteCheckBase
{
    private const string RefusalMessage =
        "This is the last enabled platform administrator. Another platform administrator must be configured first.";

    private readonly IUserRepository _userRepository;
    private readonly ILogger _logger;

    public CheckTargetIsNotPlatformAdmin(IUserRepository userRepository, ILogger<CheckTargetIsNotPlatformAdmin> logger)
    {
        _userRepository = userRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    public override int Order => 2;

    /// <summary>
    /// Fails when the target is the last enabled platform administrator, otherwise delegates to the next check.
    /// </summary>
    /// <param name="state">The target and the configured platform administrators.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The failing validation error, or <see langword="null" /> when the rule is satisfied.</returns>
    public override async Task<ApiDetailError?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var isPlatformAdmin = state.PlatformAdminRowIds.Contains(state.Target.RowId);

        if (isPlatformAdmin && await IsLastEnabledPlatformAdminAsync(state, cancellationToken))
        {
            return ApiDetailErrorFactory.CreateBadRequestError(
                nameof(Input.RowId),
                state.Target.RowId,
                RefusalMessage);
        }

        return await base.HandleAsync(state, cancellationToken);
    }

    private async Task<bool> IsLastEnabledPlatformAdminAsync(InputState state, CancellationToken cancellationToken)
    {
        var otherConfiguredRowIds = state.PlatformAdminRowIds
            .Where(rowId => rowId != state.Target.RowId)
            .ToList();

        var otherEnabledCount = await _userRepository.CountEnabledPlatformAdminsAsync(otherConfiguredRowIds, cancellationToken);

        return otherEnabledCount == 0;
    }
}
