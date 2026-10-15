using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Users.UpdateStatus.Models;
using Pot.Shared.Enumerations;

namespace Pot.App.Features.Users.UpdateStatus.EntityChecks.Checks;

/// <summary>
/// Rejects a status change that would disable a configured platform administrator.
/// </summary>
/// <remarks>
/// <c>platform:manage</c> is granted from deployment configuration and cannot be revoked within the application,
/// so a platform administrator disabled through this route has no in-app remedy.
/// </remarks>
internal sealed class CheckTargetIsNotPlatformAdmin : PreUpdateCheckBase
{
    private readonly ILogger _logger;

    public CheckTargetIsNotPlatformAdmin(ILogger<CheckTargetIsNotPlatformAdmin> logger)
    {
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    public override Task<ApiDetailError?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var isDisabling = state.Input.Status != UserStatus.Enabled;
        var isPlatformAdmin = state.PlatformAdminRowIds.Contains(state.Target.RowId);

        if (isDisabling && isPlatformAdmin)
        {
            var error = ApiDetailErrorFactory.CreateUnprocessableEntityError(
                nameof(Input.Status),
                state.Input.Status.Name,
                "A platform administrator cannot be disabled.");

            return Task.FromResult<ApiDetailError?>(error);
        }

        return base.HandleAsync(state, cancellationToken);
    }
}
