using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using Pot.AspNetCore.Concerns.Auth.Models;
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

    public AuthSessionEntity CreateSession(UserEntity user, string refreshToken, DateTime refreshTokenExpiryUtc,
        DateTime nowUtc, LoginRequestContext context, Guid sessionRowId)
    {
        _logger.LogCall(this, new { userId = user.Id });

        var authSession = new AuthSessionEntity
        {
            // Assigned by the caller instead of the RowId value generator so the access token issued for this
            // sign-in can carry the session id before the row is inserted. An explicit value is respected.
            RowId = sessionRowId,
            UserId = user.Id,
            User = user,
            RefreshTokenHash = HashRefreshToken(refreshToken),
            CreatedUtc = nowUtc,
            ExpiresUtc = refreshTokenExpiryUtc,
            LastSeenUtc = nowUtc,
            UserAgent = NormalizeValue(context.UserAgent, AuthSessionEntity.MaxUserAgentLength),
            IpAddress = NormalizeValue(context.IpAddress, AuthSessionEntity.MaxIpAddressLength)
        };

        // Staged only, deliberately: the caller saves this row together with its own changes in one
        // SaveChanges, which is what makes the login's session insert and audit update atomic. Saving here
        // would split them into two transactions.
        _authSessionRepository.Add(authSession);

        return authSession;
    }

    public async Task<AuthSessionEntity?> ResolveActiveSessionByRefreshTokenAsync(string refreshToken, DateTime nowUtc, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var refreshTokenHash = HashRefreshToken(refreshToken);

        var authSession = await _authSessionRepository
            .GetByRefreshTokenHashOrDefaultAsync(refreshTokenHash, includeUser: true, cancellationToken);

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

        var authSessions = await _authSessionRepository.GetUnrevokedByUserIdAsync(userId, cancellationToken);

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

    private static string? NormalizeValue(string? value, int maxLength)
    {
        var trimmedValue = value?.Trim();

        if (trimmedValue.IsNullOrEmpty())
        {
            return null;
        }

        return trimmedValue.Length <= maxLength
            ? trimmedValue
            : trimmedValue[..maxLength];
    }
}
