using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Users.UpdateStatus.Models;
using Pot.Data.Repositories.Users;
using Pot.Shared.Enumerations;

namespace Pot.App.Features.Users.UpdateStatus.EntityChecks.Checks;

/// <summary>
/// Rejects a status change that would disable the last enabled platform administrator.
/// </summary>
/// <remarks>
/// <c>platform:manage</c> is granted from deployment configuration and cannot be revoked within the application.
/// A configured platform administrator can therefore be disabled only while another enabled platform
/// administrator remains; the last one has no in-app remedy.
/// </remarks>
internal sealed class CheckTargetIsNotPlatformAdmin : PreUpdateCheckBase
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger _logger;

    public CheckTargetIsNotPlatformAdmin(IUserRepository userRepository, ILogger<CheckTargetIsNotPlatformAdmin> logger)
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

        if (isDisabling && isPlatformAdmin && await IsLastEnabledPlatformAdminAsync(state, cancellationToken))
        {
            var error = ApiDetailErrorFactory.CreateBadRequestError(
                nameof(Input.Status),
                "This is the last enabled platform administrator. Another platform administrator must be configured first.");

            return error;
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
