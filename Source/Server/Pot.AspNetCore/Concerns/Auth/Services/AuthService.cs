using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using AllOverIt.Patterns.Specification;
using AllOverIt.Patterns.Specification.Extensions;
using Microsoft.EntityFrameworkCore;
using Pot.App.Concerns.Auth;
using Pot.App.Concerns.Time;
using Pot.App.Errors;
using Pot.AspNetCore.Concerns.Auth.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Users;
using Pot.Shared.Enumerations;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace Pot.AspNetCore.Concerns.Auth.Services;

// AuthService flow summary (current implementation as of PRD 001/002 Step 1.2).
//
// Purpose:
// - Orchestrate login, refresh, logout, and password-change behavior.
// - Persist refresh-token lifecycle in AuthSession rows using hashed refresh tokens.
// - Keep global access-token invalidation available through User.TokenVersion.
//
// Data model notes:
// - Access token: JWT, short-lived, validated against User.TokenVersion elsewhere in the pipeline.
// - Refresh token: opaque random value sent to the client; only SHA-256 hash is stored in the database.
// - AuthSession: one row per login event; refresh rotates within the same row.
//
// Method flow map:
// - LoginAsync:
//   1) Validate username/password and account status.
//   2) Issue new access/refresh tokens.
//   3) Create AuthSession with refresh-token hash, expiry, and request metadata.
//   4) Save session + user login metadata.
// - RefreshAsync:
//   1) Optionally parse expired access token subject for consistency check.
//   2) Resolve active AuthSession by refresh-token hash.
//   3) Reject if session resolution fails or subject mismatch is detected.
//   4) Rotate tokens and update same session row (hash, expiry, last-seen).
// - LogoutAsync:
//   1) Return early if no refresh token cookie is present (anonymous/expired cookie).
//   2) Resolve the active AuthSession for this specific refresh token.
//   3) Revoke only that session row; TokenVersion is NOT incremented so other devices are unaffected.
// - ChangePasswordAsync:
//   1) Validate current password.
//   2) Persist new password hash.
//   3) Revoke all sessions for this user.
//   4) Increment TokenVersion (global access-token invalidation).
//
// Security intent:
// - No raw refresh token is persisted.
// - Refresh-token rotation is one-time-use per successful refresh.
// - Session revoke and TokenVersion are intentionally separate controls.
//
// TokenVersion explained:
// - JWTs are stateless: the server cannot revoke a live access token by deleting a record.
//   An attacker holding a stolen access token can use it until it naturally expires.
// - TokenVersion bridges that gap. The User row holds a monotonically incrementing integer.
//   Every issued JWT embeds the TokenVersion at the time of issuance. The authentication
//   middleware validates the claim on every request; if the token version in the JWT is
//   lower than the current User.TokenVersion the request is rejected immediately.
// - This provides synchronous, global revocation without a per-request session DB lookup.
// - Per-session revocation (RevokedUtc on AuthSession) only prevents future refresh calls;
//   it cannot cancel a live access token already held by a client. TokenVersion does.
// - TokenVersion is therefore incremented only on global-revoke paths:
//     * ChangePasswordAsync  — credentials compromised, all devices must re-authenticate.
//     * Any future logout-all operation.
//   It is deliberately NOT incremented on single-device logout; that path revokes only the
//   AuthSession row so other devices keep their access tokens and can refresh normally.
//
internal sealed class AuthService : IAuthService
{
    private sealed class UserIsNotDisabledSpecification : Specification<UserEntity?>
    {
        internal static UserIsNotDisabledSpecification Instance = new();

        public override bool IsSatisfiedBy(UserEntity? candidate)
        {
            var status = candidate?.Status;

            return status is not null && status != UserStatus.Disabled;
        }
    }

    private sealed class UserProvidedCorrectPasswordSpecification : Specification<UserEntity?>
    {
        private readonly IUserPasswordHasher _passwordHasher;
        private readonly string _password;

        public UserProvidedCorrectPasswordSpecification(IUserPasswordHasher passwordHasher, string password)
        {
            _passwordHasher = passwordHasher.WhenNotNull();
            _password = password;
        }

        public override bool IsSatisfiedBy(UserEntity? candidate)
        {
            if (candidate is null)
            {
                return false;
            }

            return _passwordHasher.IsValidPasswordHash(candidate, _password, candidate.PasswordHash);
        }
    }

    private const int RefreshTokenExpiryDays = 30;

    private readonly IJwtService _jwtService;
    private readonly IAuthSessionService _authSessionService;
    private readonly IUserPasswordHasher _passwordHasher;
    private readonly IPersistableUserRepository _userRepository;
    private readonly ITimeProvider _timeProvider;
    private readonly ILogger _logger;

    public AuthService(IJwtService jwtService, IAuthSessionService authSessionService, IUserPasswordHasher passwordHasher,
        IPersistableUserRepository userRepository, ITimeProvider timeProvider, ILogger<AuthService> logger)
    {
        _jwtService = jwtService.WhenNotNull();
        _authSessionService = authSessionService.WhenNotNull();
        _passwordHasher = passwordHasher.WhenNotNull();
        _userRepository = userRepository.WhenNotNull();
        _timeProvider = timeProvider.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<AuthTokens?>> LoginAsync(string username, string password, LoginRequestContext context,
        CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { username });

        using (_userRepository.WithTracking())
        {
            // 1) Resolve the user and validate credentials/state.
            var user = await _userRepository.Users
                .SingleOrDefaultAsync(user => user.Username == username, cancellationToken)
                .ConfigureAwait(false);

            if (user is null)
            {
                return CreateAuthError();
            }

            var userSpecification = CreateValidUserSpecification(_passwordHasher, password);
            var credentialsValid = userSpecification.IsSatisfiedBy(user);

            if (!credentialsValid)
            {
                return CreateAuthError();
            }

            // Auto-enable invited users on first login
            if (user.Status == UserStatus.Pending)
            {
                user.Status = UserStatus.Enabled;
            }

            // Return pending approval status for users awaiting approval
            if (user.Status == UserStatus.Approval)
            {
                // Return null tokens to indicate no authentication, service layer will handle response
                return EnrichedResult.Success<AuthTokens?>(null);
            }

            // 2) Issue a new access token + refresh token pair for this login.
            var authTokens = CreateUserAuthTokens(user);

            var nowUtc = _timeProvider.GetUtcDateTimeNow();

            // 3) Persist a dedicated AuthSession for this login so refresh lifecycle is per-session.
            _authSessionService.CreateSession(user, authTokens.RefreshToken, authTokens.RefreshTokenExpiryUtc, nowUtc, context);

            // 4) Update login audit field and commit atomically with session creation.
            user.LastLoggedInUtc = nowUtc;

            await _userRepository
                .SaveAsync(cancellationToken)
                .ConfigureAwait(false);

            return EnrichedResult.Success<AuthTokens?>(authTokens);
        }
    }

    public async Task<EnrichedResult<bool>> LogoutAsync(Guid userId, string? refreshToken, CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { userId });

        // No refresh token means no active session to revoke (e.g., anonymous or already expired cookie).
        if (refreshToken.IsNullOrEmpty())
        {
            return EnrichedResult.Success(false);
        }

        using (_userRepository.WithTracking())
        {
            var nowUtc = _timeProvider.GetUtcDateTimeNow();

            // 1) Resolve the active session for this refresh token only. Must be inside WithTracking so
            //    the entity is change-tracked and the revocation mutation is detected by SaveChangesAsync.
            var authSession = await _authSessionService
                .ResolveActiveSessionByRefreshTokenAsync(refreshToken!, nowUtc, cancellationToken)
                .ConfigureAwait(false);

            // Session not found (already revoked, expired, or token mismatch) - treat as a no-op.
            if (authSession is null)
            {
                return EnrichedResult.Success(false);
            }

            // 2) Revoke only this session row. TokenVersion is NOT incremented: other devices remain unaffected.
            _authSessionService.RevokeCurrentSession(authSession, nowUtc);

            await _userRepository
                .SaveAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        return EnrichedResult.Success(true);
    }

    public async Task<EnrichedResult<AuthTokens?>> RefreshAsync(string? accessToken, string refreshToken, CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { refreshToken });

        Guid? accessTokenUserRowId = null;

        if (accessToken.IsNotNullOrEmpty())
        {
            // Optional guard: if an expired access token is present, extract subject for consistency checks.
            var principal = _jwtService.GetPrincipalFromExpiredToken(accessToken);

            var subject = principal.Claims.SingleOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub);

            if (subject is null || !Guid.TryParse(subject.Value, out var userRowId))
            {
                return CreateAuthError();
            }

            accessTokenUserRowId = userRowId;
        }

        using (_userRepository.WithTracking())
        {
            var nowUtc = _timeProvider.GetUtcDateTimeNow();

            // 1) Resolve active session by refresh-token hash (DB stores only hashes, not raw token values).
            var authSession = await _authSessionService
                .ResolveActiveSessionByRefreshTokenAsync(refreshToken, nowUtc, cancellationToken)
                .ConfigureAwait(false);

            if (authSession is null)
            {
                // Unknown refresh token or known session that is no longer active.
                return CreateAuthError();
            }

            if (accessTokenUserRowId is not null && authSession.User.RowId != accessTokenUserRowId.Value)
            {
                // Prevent token-mix attacks where access-token subject and refresh-token session disagree.
                return CreateAuthError();
            }

            // 2) Rotate tokens on every successful refresh.
            var authTokens = CreateUserAuthTokens(authSession.User);

            // 3) Persist new refresh-token hash/expiry and last-seen metadata for this same session row.
            _authSessionService.RotateRefreshToken(authSession, authTokens.RefreshToken, authTokens.RefreshTokenExpiryUtc, nowUtc);

            await _userRepository
                .SaveAsync(cancellationToken)
                .ConfigureAwait(false);

            return EnrichedResult.Success<AuthTokens?>(authTokens);
        }
    }

    public async Task<EnrichedResult<bool>> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        using (_userRepository.WithTracking())
        {
            // 1) Load user and verify current password.
            var user = await _userRepository.Users
                .SingleOrDefaultAsync(user => user.RowId == userId, cancellationToken)
                .ConfigureAwait(false);

            Throw<UnreachableException>.WhenNull(user, "The user should exist");

            var isValidHash = _passwordHasher.IsValidPasswordHash(user, currentPassword, user.PasswordHash);

            if (!isValidHash)
            {
                var authError = ApiDetailErrorFactory.CreateAuthError("The current password is invalid");
                return EnrichedResult.Fail<bool>(authError);
            }

            // 2) Write new password hash.
            user.PasswordHash = _passwordHasher.GetHash(user, newPassword);

            var nowUtc = _timeProvider.GetUtcDateTimeNow();

            // 3) Revoke all sessions for this user.
            _ = await _authSessionService
                .RevokeAllSessionsForUserAsync(user.Id, nowUtc, cancellationToken)
                .ConfigureAwait(false);

            // 4) Invalidate all existing access tokens.
            user.TokenVersion++;

            await _userRepository
                .SaveAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        return EnrichedResult.Success(true);
    }

    private AuthTokens CreateUserAuthTokens(UserEntity user)
    {
        var accessToken = _jwtService.CreateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        // Expire the refresh token at midnight in the user's local time zone to minimise risk of expiring mid-session
        var refreshTokenExpiry = _timeProvider
            .GetLocalDateNow()
            .AddDays(RefreshTokenExpiryDays)
            .ToDateTime(TimeOnly.MinValue)
            .ToUniversalTime(); // Stored as UTC in the database

        return new AuthTokens(accessToken, refreshToken, refreshTokenExpiry);
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber);
    }

    private static EnrichedResult<AuthTokens?> CreateAuthError()
    {
        // This also applies to attempting to refresh an access token with a refresh token that has expired,
        // and users that are disabled.
        var loginProblem = ApiDetailErrorFactory.CreateAuthError("Invalid username or password");

        return EnrichedResult.Fail<AuthTokens?>(loginProblem);
    }

    private static ISpecification<UserEntity?> CreateValidUserSpecification(IUserPasswordHasher passwordHasher, string password)
    {
        return UserIsNotDisabledSpecification.Instance.And(new UserProvidedCorrectPasswordSpecification(passwordHasher, password));
    }
}
