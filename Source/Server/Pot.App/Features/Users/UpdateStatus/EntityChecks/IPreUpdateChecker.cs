using Pot.App.Errors;
using Pot.App.Features.Users.UpdateStatus.Models;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Users.UpdateStatus.EntityChecks;

/// <summary>
/// Validates that a user's status can be changed before the change is persisted.
/// </summary>
public interface IPreUpdateChecker : IPotScopedDependency
{
    /// <summary>
    /// Runs the registered pre-update checks for the supplied user and reports the first failure.
    /// </summary>
    /// <remarks>
    /// Checks are evaluated in sequence and stop at the first failure, so any checks later in the chain are not
    /// executed once an earlier check fails.
    /// </remarks>
    /// <param name="input">The requested status change.</param>
    /// <param name="userToUpdate">The stored user whose status is changing.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <see cref="ApiDetailError"/> describing the first failed check, or <see langword="null" /> when every check
    /// passes.
    /// </returns>
    Task<ApiDetailError?> CanSaveAsync(Input input, UserEntity userToUpdate, CancellationToken cancellationToken);
}
