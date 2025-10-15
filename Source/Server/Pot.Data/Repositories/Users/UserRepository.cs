using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;

namespace Pot.Data.Repositories.Users;

internal sealed class UserRepository : GenericRepository<PotDbContext, UserEntity>, IPersistableUserRepository
{
    public UserRepository(PotDbContext dbContext)
        : base(dbContext)
    {
    }

    public Task<UserEntity?> GetByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        return Current
            .Where(user => user.Username == username)
            .SingleOrDefaultAsync(cancellationToken);
    }
}