using Pot.AspNetCore.Concerns.Auth.Models;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.AspNetCore.Concerns.Auth.Services;

public interface IAuthSessionService : IPotScopedDependency
{
    AuthSessionEntity CreateSession(UserEntity user, string refreshToken, DateTime refreshTokenExpiryUtc, DateTime nowUtc, LoginRequestContext context);
    Task<AuthSessionEntity?> ResolveActiveSessionByRefreshTokenAsync(string refreshToken, DateTime nowUtc, CancellationToken cancellationToken);
    void RotateRefreshToken(AuthSessionEntity authSession, string refreshToken, DateTime refreshTokenExpiryUtc, DateTime nowUtc);
    void RevokeCurrentSession(AuthSessionEntity authSession, DateTime nowUtc);
    Task<int> RevokeAllSessionsForUserAsync(int userId, DateTime nowUtc, CancellationToken cancellationToken);
}
