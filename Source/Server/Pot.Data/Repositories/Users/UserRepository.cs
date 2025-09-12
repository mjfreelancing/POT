using Pot.Data.Entities;

namespace Pot.Data.Repositories.Users;

internal sealed class UserRepository : GenericRepository<PotDbContext, UserEntity>, IPersistableUserRepository
{
    public UserRepository(PotDbContext dbContext) : base(dbContext)
    {
    }
}