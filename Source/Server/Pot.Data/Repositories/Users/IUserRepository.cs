using Pot.Data.Entities;

namespace Pot.Data.Repositories.Users;

public interface IUserRepository : IRepositoryBase
{
    IQueryable<UserEntity> Users { get; }

    UserEntity GetCurrentUser(bool includeSite);
    Task<UserEntity?> GetByUsernameOrDefaultAsync(string username, CancellationToken cancellationToken);
}
