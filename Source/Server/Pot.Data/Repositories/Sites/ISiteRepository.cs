using Pot.Data.Entities;

namespace Pot.Data.Repositories.Sites;

/// <summary>
/// Reads and writes sites.
/// </summary>
/// <remarks>
/// Do not add an <see cref="IQueryable{T}" /> member of <see cref="SiteEntity" /> to this interface, and do not
/// reach for <c>Set&lt;SiteEntity&gt;()</c> from a service. A site is always resolved from the current user
/// context rather than from an id supplied by a caller, so a caller cannot name another site's row at all.
/// </remarks>
public interface ISiteRepository : IRepositoryBase
{
    /// <summary>
    /// Resolves the signed-in caller's site.
    /// </summary>
    /// <returns>The caller's site.</returns>
    SiteEntity GetCurrentSite();
}
