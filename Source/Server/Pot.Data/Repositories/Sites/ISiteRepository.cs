using Pot.Data.Entities;

namespace Pot.Data.Repositories.Sites;

public interface ISiteRepository : IRepositoryBase
{
    SiteEntity GetCurrentSite();
}
