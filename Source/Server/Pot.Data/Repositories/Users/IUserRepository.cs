using Pot.Data.Entities;
using Pot.Data.Repositories.Users.Dtos;

namespace Pot.Data.Repositories.Users;

public interface IUserRepository : IRepositoryBase
{
    IQueryable<UserEntity> Users { get; }
    IQueryable<AuthSessionEntity> AuthSessions { get; }

    UserEntity GetCurrentUser(bool includeSite);
    Task<List<GetAllUserInfo>> GetEnabledUsersAsync(CancellationToken cancellationToken);
    Task<List<GetAllUserInfo>> GetAllForCurrentSiteAsync(CancellationToken cancellationToken);
    Task<UserEntity?> GetByUsernameOrDefaultAsync(string username, CancellationToken cancellationToken);
}
