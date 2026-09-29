using Pot.App.Errors;
using Pot.App.Features.Approvals.UpdateStatus.Models;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Approvals.UpdateStatus.EntityChecks;

/// <summary>
/// Validates that a user approval update can be saved.
/// </summary>
/// <remarks>
/// Checks are evaluated in sequence and evaluation stops at the first failure, so no more than one error is
/// returned for a single attempt.
/// </remarks>
public interface IPreUpdateChecker : IPotScopedDependency
{
    /// <summary>
    /// Determines whether the requested approval change can be saved against the supplied user.
    /// </summary>
    /// <param name="request">The requested approval changes.</param>
    /// <param name="user">The persisted user that will be modified.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The failing validation error, or <see langword="null"/> when the update can be saved.</returns>
    Task<ApiDetailError?> CanSaveAsync(Input request, UserEntity user, CancellationToken cancellationToken);
}
