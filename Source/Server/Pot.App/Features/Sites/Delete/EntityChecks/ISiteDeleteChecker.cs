using Pot.App.Errors;
using Pot.App.Features.Sites.Delete.Models;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Sites.Delete.EntityChecks;

/// <summary>
/// Validates a site deletion before any row is removed.
/// </summary>
public interface ISiteDeleteChecker : IPotScopedDependency
{
    /// <summary>
    /// Runs the deletion rules for the caller's site.
    /// </summary>
    /// <param name="input">The typed site name, username, confirmation password and configured platform administrators.</param>
    /// <param name="caller">The signed-in user.</param>
    /// <param name="site">The caller's site.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The first failing error, or <see langword="null" /> when every rule is satisfied.</returns>
    Task<ApiDetailError?> CanDeleteAsync(Input input, UserEntity caller, SiteEntity site, CancellationToken cancellationToken);
}
