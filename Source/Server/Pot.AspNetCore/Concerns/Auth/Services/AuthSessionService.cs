using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Pot.Data.Entities;
using Pot.Data.Repositories.AuthSessions;
using System.Security.Cryptography;
using System.Text;

namespace Pot.AspNetCore.Concerns.Auth.Services;

internal sealed class AuthSessionService : IAuthSessionService
{
    private readonly IPersistableAuthSessionRepository _authSessionRepository;
    private readonly ILogger<AuthSessionService> _logger;

    public AuthSessionService(IPersistableAuthSessionRepository authSessionRepository, ILogger<AuthSessionService> logger)
    {
        _authSessionRepository = authSessionRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public AuthSessionEntity CreateSession(UserEntity user, string refreshToken, DateTime refreshTokenExpiryUtc, DateTime nowUtc)
    {
        _logger.LogCall(this, new { userId = user.Id });

        var authSession = new AuthSessionEntity
        {
            UserId = user.Id,
            User = user,
            RefreshTokenHash = HashRefreshToken(refreshToken),
            CreatedUtc = nowUtc,
            ExpiresUtc = refreshTokenExpiryUtc,
            LastSeenUtc = nowUtc
        };

        _authSessionRepository.Add(authSession);

        return authSession;
    }

    public async Task<AuthSessionEntity?> ResolveActiveSessionByRefreshTokenAsync(string refreshToken, DateTime nowUtc, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var refreshTokenHash = HashRefreshToken(refreshToken);

        var authSession = await _authSessionRepository
            .GetByRefreshTokenHashOrDefaultAsync(refreshTokenHash, includeUser: true, cancellationToken)
            .ConfigureAwait(false);

        if (authSession is null)
        {
            return null;
        }

        if (authSession.RevokedUtc is not null || authSession.ExpiresUtc <= nowUtc)
        {
            return null;
        }

        return authSession;
    }

    public void RotateRefreshToken(AuthSessionEntity authSession, string refreshToken, DateTime refreshTokenExpiryUtc, DateTime nowUtc)
    {
        _logger.LogCall(this, new { sessionRowId = authSession.RowId });

        authSession.RefreshTokenHash = HashRefreshToken(refreshToken);
        authSession.ExpiresUtc = refreshTokenExpiryUtc;
        authSession.LastSeenUtc = nowUtc;
    }

    public void RevokeCurrentSession(AuthSessionEntity authSession, DateTime nowUtc)
    {
        _logger.LogCall(this, new { sessionRowId = authSession.RowId });

        authSession.RevokedUtc = nowUtc;
        authSession.LastSeenUtc = nowUtc;
    }

    public async Task<int> RevokeAllSessionsForUserAsync(int userId, DateTime nowUtc, CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { userId });

        var authSessions = await _authSessionRepository
            .GetUnrevokedByUserIdAsync(userId, cancellationToken)
            .ConfigureAwait(false);

        foreach (var authSession in authSessions)
        {
            RevokeCurrentSession(authSession, nowUtc);
        }

        return authSessions.Count;
    }

    private static string HashRefreshToken(string refreshToken)
    {
        var refreshTokenBytes = Encoding.UTF8.GetBytes(refreshToken);
        var refreshTokenHash = SHA256.HashData(refreshTokenBytes);

        return Convert.ToHexString(refreshTokenHash);
    }
}
