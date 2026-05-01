using Pot.Data.Entities;

namespace Pot.Data.Repositories.AuthSessions;

public interface IAuthSessionRepository : IRepositoryBase
{
    Task<AuthSessionEntity?> GetByRefreshTokenHashOrDefaultAsync(string refreshTokenHash, bool includeUser, CancellationToken cancellationToken);
    Task<List<AuthSessionEntity>> GetUnrevokedByUserIdAsync(int userId, CancellationToken cancellationToken);
}
