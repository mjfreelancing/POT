using Pot.App.Errors;
using Pot.App.Features.Users.Delete.Models;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Users.Delete.EntityChecks;

/// <summary>
/// Validates a user deletion before any row is removed.
/// </summary>
public interface IUserDeleteChecker : IPotScopedDependency
{
    /// <summary>
    /// Runs the deletion rules for the caller and the target.
    /// </summary>
    /// <param name="input">The target, the confirmation password and the configured platform administrators.</param>
    /// <param name="caller">The signed-in caller.</param>
    /// <param name="target">The user being deleted, already resolved within the caller's site.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The first failing error, or <see langword="null" /> when every rule is satisfied.</returns>
    Task<ApiDetailError?> CanDeleteAsync(Input input, UserEntity caller, UserEntity target, CancellationToken cancellationToken);
}
