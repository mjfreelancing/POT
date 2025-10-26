using AllOverIt.Assertion;
using Pot.Data.Entities;
using Pot.Data.Repositories.Users;

namespace Pot.Data.Repositories.Sites;

internal sealed class SiteRepository : PersistableRepository, IPersistableSiteRepository
{
    private readonly IUserRepository _userRepository;

    public IQueryable<SiteEntity> Sites => Set<SiteEntity>();

    public SiteRepository(PotDbContext dbContext, IUserRepository userRepository)
        : base(dbContext)
    {
        _userRepository = userRepository.WhenNotNull(); ;
    }

    public SiteEntity GetCurrentSite()
    {
        return _userRepository.GetCurrentUser(true).Site;
    }
}
