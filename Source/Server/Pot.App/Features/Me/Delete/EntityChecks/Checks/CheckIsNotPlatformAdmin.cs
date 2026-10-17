using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Me.Delete.Models;
using Pot.Data.Repositories.Users;

namespace Pot.App.Features.Me.Delete.EntityChecks.Checks;

/// <summary>
/// Rejects an account deletion when the caller is the last live platform administrator.
/// </summary>
/// <remarks>
/// Runs before <see cref="CheckIsNotLastSiteAdmin" />, so a caller protected by both rules sees the refusal
/// that has no in-app remedy.
/// </remarks>
internal sealed class CheckIsNotPlatformAdmin : AccountDeleteCheckBase
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger _logger;

    public CheckIsNotPlatformAdmin(IUserRepository userRepository, ILogger<CheckIsNotPlatformAdmin> logger)
    {
        _userRepository = userRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    public override int Order => 2;

    /// <summary>
    /// Fails when the caller is the last enabled platform administrator, otherwise delegates to the next check.
    /// </summary>
    /// <param name="state">The caller and the configured platform administrators.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The failing validation error, or <see langword="null" /> when the rule is satisfied.</returns>
    public override async Task<ApiDetailError?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var isPlatformAdmin = state.PlatformAdminRowIds.Contains(state.Caller.RowId);

        if (isPlatformAdmin && await IsLastEnabledPlatformAdminAsync(state, cancellationToken))
        {
            return ApiDetailErrorFactory.CreateBadRequestError(
                nameof(Input.Username),
                state.Caller.RowId,
                "This is the last enabled platform administrator. Another platform administrator must be configured first.");
        }

        return await base.HandleAsync(state, cancellationToken);
    }

    private async Task<bool> IsLastEnabledPlatformAdminAsync(InputState state, CancellationToken cancellationToken)
    {
        var otherConfiguredRowIds = state.PlatformAdminRowIds
            .Where(rowId => rowId != state.Caller.RowId)
            .ToList();

        var otherEnabledCount = await _userRepository.CountEnabledPlatformAdminsAsync(otherConfiguredRowIds, cancellationToken);

        return otherEnabledCount == 0;
    }
}
