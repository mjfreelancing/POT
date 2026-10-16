using Pot.AspNetCore.Concerns.Auth.Models;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.AspNetCore.Concerns.Auth.Services;

/// <summary>Defines operations for creating, resolving, rotating and revoking authentication sessions.</summary>
public interface IAuthSessionService : IPotScopedDependency
{
    /// <summary>Creates an authentication session for the specified user.</summary>
    /// <param name="user">The user associated with the authentication session.</param>
    /// <param name="refreshToken">The refresh token associated with the session.</param>
    /// <param name="refreshTokenExpiryUtc">The UTC time when the refresh token expires.</param>
    /// <param name="nowUtc">The current time in UTC.</param>
    /// <param name="context">The user-agent and client address associated with this sign-in attempt.</param>
    AuthSessionEntity CreateSession(UserEntity user, string refreshToken, DateTime refreshTokenExpiryUtc, DateTime nowUtc, LoginRequestContext context);
    /// <summary>Finds the active session associated with a refresh token.</summary>
    /// <param name="refreshToken">The refresh token associated with the session.</param>
    /// <param name="nowUtc">The current time in UTC.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    Task<AuthSessionEntity?> ResolveActiveSessionByRefreshTokenAsync(string refreshToken, DateTime nowUtc, CancellationToken cancellationToken);
    /// <summary>Replaces the refresh token and expiry stored for a session.</summary>
    /// <param name="authSession">The authentication session to update.</param>
    /// <param name="refreshToken">The refresh token associated with the session.</param>
    /// <param name="refreshTokenExpiryUtc">The UTC time when the refresh token expires.</param>
    /// <param name="nowUtc">The current time in UTC.</param>
    void RotateRefreshToken(AuthSessionEntity authSession, string refreshToken, DateTime refreshTokenExpiryUtc, DateTime nowUtc);
    /// <summary>Revokes the specified authentication session.</summary>
    /// <param name="authSession">The authentication session to update.</param>
    /// <param name="nowUtc">The current time in UTC.</param>
    void RevokeCurrentSession(AuthSessionEntity authSession, DateTime nowUtc);
    /// <summary>Revokes all authentication sessions belonging to a user.</summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="nowUtc">The current time in UTC.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>The number of sessions revoked for the user.</returns>
    Task<int> RevokeAllSessionsForUserAsync(int userId, DateTime nowUtc, CancellationToken cancellationToken);
}
