using Pot.Data.Repositories.Sites.Dtos;

namespace Pot.Data.Repositories.Sites;

/// <summary>
/// Reads and writes sites.
/// </summary>
public interface IPersistableSiteRepository : ISiteRepository, IPersistableRepository
{
    /// <summary>
    /// Removes the caller's site and every record that belongs to it.
    /// </summary>
    /// <remarks>
    /// The removal is ordered by the foreign keys: expenses, incomes, accounts, settings, one-time codes,
    /// sessions, users, then the site.
    /// </remarks>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The removed counts.</returns>
    Task<SiteDeletionCounts> DeleteCurrentSiteWithDependentsAsync(CancellationToken cancellationToken);
}