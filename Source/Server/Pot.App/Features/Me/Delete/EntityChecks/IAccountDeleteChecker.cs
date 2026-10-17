using Pot.App.Errors;
using Pot.App.Features.Me.Delete.Models;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Me.Delete.EntityChecks;

/// <summary>
/// Validates an account self-deletion before any row is removed.
/// </summary>
public interface IAccountDeleteChecker : IPotScopedDependency
{
    /// <summary>
    /// Runs the deletion rules for the caller.
    /// </summary>
    /// <param name="input">The typed username, the confirmation password and the configured platform administrators.</param>
    /// <param name="caller">The signed-in user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The first failing error, or <see langword="null" /> when every rule is satisfied.</returns>
    Task<ApiDetailError?> CanDeleteAsync(Input input, UserEntity caller, CancellationToken cancellationToken);
}