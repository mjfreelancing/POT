using AllOverIt.Patterns.Result;
using Pot.App.Features.Users.Delete.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Users.Delete;

/// <summary>
/// Deletes a user who belongs to the caller's site.
/// </summary>
public interface IDeleteUserService : IPotScopedDependency
{
    /// <summary>
    /// Removes the target user together with their sessions and one-time codes, after the deletion rules and the
    /// caller's confirmation password have been verified.
    /// </summary>
    /// <param name="input">The target, the confirmation password and the configured platform administrators.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A successful result, or the first refusal or not-found outcome.</returns>
    Task<EnrichedResult<bool>> DeleteUserAsync(Input input, CancellationToken cancellationToken);
}
