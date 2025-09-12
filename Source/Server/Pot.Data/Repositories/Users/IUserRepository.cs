using Pot.Data.Entities;

namespace Pot.Data.Repositories.Users;

public interface IUserRepository : IGenericRepository<PotDbContext, UserEntity>
{
}
