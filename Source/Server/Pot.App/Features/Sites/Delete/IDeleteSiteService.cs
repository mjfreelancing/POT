using AllOverIt.Patterns.Result;
using Pot.App.Features.Sites.Delete.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Sites.Delete;

/// <summary>
/// Deletes the caller's site and everything that belongs to it.
/// </summary>
public interface IDeleteSiteService : IPotScopedDependency
{
    /// <summary>
    /// Removes the caller's site, its users, sessions, accounts, expenses, incomes and settings, after the
    /// deletion rules and the confirmation values have been verified.
    /// </summary>
    /// <param name="input">The typed site name and username, the confirmation password and the configured platform administrators.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A successful result, or the first refusal.</returns>
    Task<EnrichedResult<bool>> DeleteSiteAsync(Input input, CancellationToken cancellationToken);
}
