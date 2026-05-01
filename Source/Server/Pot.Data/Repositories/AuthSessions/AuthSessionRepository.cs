using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;

namespace Pot.Data.Repositories.AuthSessions;

internal sealed class AuthSessionRepository : PersistableRepository, IPersistableAuthSessionRepository
{
    public AuthSessionRepository(PotDbContext dbContext)
        : base(dbContext)
    {
    }

    public Task<AuthSessionEntity?> GetByRefreshTokenHashOrDefaultAsync(string refreshTokenHash, bool includeUser, CancellationToken cancellationToken)
    {
        var authSessionQuery = Set<AuthSessionEntity>().Where(authSession => authSession.RefreshTokenHash == refreshTokenHash);

        if (includeUser)
        {
            authSessionQuery = authSessionQuery.Include(authSession => authSession.User);
        }

        return authSessionQuery.SingleOrDefaultAsync(cancellationToken);
    }

    public Task<List<AuthSessionEntity>> GetUnrevokedByUserIdAsync(int userId, CancellationToken cancellationToken)
    {
        return Set<AuthSessionEntity>()
            .Where(authSession => authSession.UserId == userId && authSession.RevokedUtc == null)
            .ToListAsync(cancellationToken);
    }
}
