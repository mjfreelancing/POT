using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.Data.Repositories.Users;
using Pot.Shared.Enumerations;

namespace Pot.App.Features.Users.UpdateRoles.EntityChecks.Checks;

/// <summary>
/// Rejects a role change that would leave the target's site without an enabled administrator by removing the
/// <c>Admin</c> role from its only holder.
/// </summary>
/// <remarks>
/// A site role is not otherwise protected: only the enabled <c>Admin</c> population is, and only when the change
/// would empty it. Removing <c>Admin</c> from someone while another enabled administrator remains is allowed.
/// </remarks>
internal sealed class CheckRemovingAdminLeavesAnAdministrator : PreUpdateCheckBase
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger _logger;

    public CheckRemovingAdminLeavesAnAdministrator(IUserRepository userRepository, ILogger<CheckRemovingAdminLeavesAnAdministrator> logger)
    {
        _userRepository = userRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    public override async Task<ApiDetailError?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        if (RemovesAdmin(state) && await IsOnlyEnabledSiteAdminAsync(state, cancellationToken))
        {
            var error = ApiDetailErrorFactory.CreateBadRequestError(
                nameof(InputState.RoleIds),
                "This is the site's only enabled administrator. Promote another administrator first.");

            return error;
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

    private static bool RemovesAdmin(InputState state)
    {
        var currentAdminRole = state.Target.Roles.FirstOrDefault(role => role.Name == Role.Admin);

        // Nothing is lost when the target does not currently hold Admin, or when the requested roles still
        // include it.
        return currentAdminRole is not null && !state.Input.RoleIds.Contains(currentAdminRole.RowId);
    }
}
