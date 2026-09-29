using AllOverIt.Patterns.Result;
using Pot.App.Features.Sites.Update.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Sites.Update;

/// <summary>
/// Updates the details of an existing site.
/// </summary>
public interface IUpdateSiteService : IPotScopedDependency
{
    /// <summary>
    /// Updates the name and description of the site identified by the input.
    /// </summary>
    /// <remarks>
    /// The site's concurrency eTag is verified before the change is persisted, and an empty description is stored as
    /// <see langword="null" />.
    /// </remarks>
    /// <param name="input">The site to update and the values to apply.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <c>EnrichedResult&lt;Output&gt;</c> containing the updated site, or a failure when the site does not exist
    /// or the eTag does not match.
    /// </returns>
    Task<EnrichedResult<Output>> UpdateSiteAsync(Input input, CancellationToken cancellationToken);
}
