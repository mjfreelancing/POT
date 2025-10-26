using AllOverIt.Assertion;
using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;
using Pot.Shared;

namespace Pot.Data.Repositories.Users;

internal sealed class UserRepository : PersistableRepository, IPersistableUserRepository
{
    private readonly ICurrentUserContext _currentUserContext;

    public IQueryable<UserEntity> Users => Set<UserEntity>();

    public UserRepository(PotDbContext dbContext, ICurrentUserContext currentUserContext)
        : base(dbContext)
    {
        _currentUserContext = currentUserContext.WhenNotNull();
    }

    public UserEntity GetCurrentUser(bool includeSite)
    {
        var query = Users;

        if (includeSite)
        {
            query = query.Include(user => user.Site);
        }

        return query.Single(user => user.RowId == _currentUserContext.UserRowId);
    }

    public Task<UserEntity?> GetByUsernameOrDefaultAsync(string username, CancellationToken cancellationToken)
    {
        return Users.SingleOrDefaultAsync(user => user.Username == username, cancellationToken);
    }
}
