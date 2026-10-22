using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Users.UpdateStatus.Models;
using Pot.Data.Repositories.Users;
using Pot.Shared.Enumerations;

namespace Pot.App.Features.Users.UpdateStatus.EntityChecks.Checks;

/// <summary>
/// Rejects a status change that would leave the target's site without an enabled administrator.
/// </summary>
/// <remarks>
/// The population counted is the enabled <c>Admin</c>-role holders of the site, resolved by
/// <see cref="IUserRepository.GetEnabledAdminsForCurrentSiteAsync" />, so every feature that protects a tenant's
/// administrability counts the same people.
/// </remarks>
internal sealed class CheckTargetIsNotLastEnabledSiteAdmin : PreUpdateCheckBase
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger _logger;

    public CheckTargetIsNotLastEnabledSiteAdmin(IUserRepository userRepository, ILogger<CheckTargetIsNotLastEnabledSiteAdmin> logger)
    {
        _userRepository = userRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    public override async Task<ApiDetailError?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var isDisabling = state.Input.Status != UserStatus.Enabled;
        var isPlatformAdmin = state.PlatformAdminRowIds.Contains(state.Target.RowId);

        if (isDisabling && await IsOnlyEnabledSiteAdminAsync(state, cancellationToken))
        {
            // The platform-admin rule decides whether a configured platform administrator can change state at
            // all, and the order the checks run in is decided by registration rather than declared, so this one
            // defers to it explicitly instead of relying on that order. Defer only while that rule refuses; once
            // it allows, the site must still keep an enabled administrator.
            var platformRuleRefuses = isPlatformAdmin && await IsLastEnabledPlatformAdminAsync(state, cancellationToken);

            if (!platformRuleRefuses)
            {
                var error = ApiDetailErrorFactory.CreateBadRequestError(
                    nameof(Input.Status),
                    "This is the site's only enabled administrator. Promote or enable another administrator first.");

                return error;
            }
        }

        return await base.HandleAsync(state, cancellationToken);
    }

    private async Task<bool> IsOnlyEnabledSiteAdminAsync(InputState state, CancellationToken cancellationToken)
    {
        var enabledAdmins = await _userRepository.GetEnabledAdminsForCurrentSiteAsync(cancellationToken);

        // Exactly one enabled administrator, and it is the row being changed. A target that is not in the
        // population, or a population larger than one, leaves the site administrable.
        return enabledAdmins.Count == 1 && enabledAdmins[0].RowId == state.Target.RowId;
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
