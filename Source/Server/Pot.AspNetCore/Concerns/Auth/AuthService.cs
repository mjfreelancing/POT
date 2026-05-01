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
using System.Text;

namespace Pot.AspNetCore.Concerns.Auth;

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
//   3) Create AuthSession with refresh-token hash and expiry.
//   4) Save session + user login metadata.
// - RefreshAsync:
//   1) Optionally parse expired access token subject for consistency check.
//   2) Find AuthSession by refresh-token hash.
//   3) Reject if session is revoked/expired or subject mismatch is detected.
//   4) Rotate tokens and update same session row (hash, expiry, last-seen).
// - LogoutAsync (CURRENT BEHAVIOR):
//   1) Revoke all active sessions for the user.
//   2) Clear legacy user-level refresh fields.
//   3) Increment TokenVersion (global access-token invalidation).
// - ChangePasswordAsync:
//   1) Validate current password.
//   2) Persist new password hash.
//   3) Revoke all sessions + clear legacy refresh fields.
//   4) Increment TokenVersion (global access-token invalidation).
//
// Security intent:
// - No raw refresh token is persisted.
// - Refresh-token rotation is one-time-use per successful refresh.
// - Session revoke and TokenVersion are intentionally separate controls.
//
// TODO (Step 2.4): Update this summary after current-session logout is implemented.
// Expected future logout behavior:
// - Revoke only the current AuthSession.
// - Do not increment TokenVersion for normal logout.
// - Keep TokenVersion increment on global-revoke flows (e.g., password change).
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

    private readonly IPersistableUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IUserPasswordHasher _passwordHasher;
    private readonly ITimeProvider _timeProvider;
    private readonly ILogger _logger;

    public AuthService(IJwtService jwtService, IUserPasswordHasher passwordHasher, IPersistableUserRepository userRepository,
        ITimeProvider timeProvider, ILogger<AuthService> logger)
    {
        _userRepository = userRepository.WhenNotNull();
        _jwtService = jwtService.WhenNotNull();
        _passwordHasher = passwordHasher.WhenNotNull();
        _timeProvider = timeProvider.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<AuthTokens?>> LoginAsync(string username, string password, CancellationToken cancellationToken)
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

            if (!userSpecification.IsSatisfiedBy(user))
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

            // 3) Persist a dedicated AuthSession for this login so refresh lifecycle is per-session.
            var authSession = CreateAuthSession(user, authTokens);
            _userRepository.Add(authSession);

            // 4) Update login audit field and commit atomically with session creation.
            user.LastLoggedInUtc = _timeProvider.GetUtcDateTimeNow();

            await _userRepository
                .SaveAsync(cancellationToken)
                .ConfigureAwait(false);

            return EnrichedResult.Success<AuthTokens?>(authTokens);
        }
    }

    public async Task<EnrichedResult<bool>> LogoutAsync(Guid userId, CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { userId });

        using (_userRepository.WithTracking())
        {
            // 1) Load the currently authenticated user row.
            var user = await _userRepository.Users
                .SingleOrDefaultAsync(user => user.RowId == userId, cancellationToken)
                .ConfigureAwait(false);

            if (user is null)
            {
                // Not doing anything with this result - merely indicating the user is not logged out since not found)
                return EnrichedResult.Success(false);
            }

            // 2) Revoke all active AuthSession rows for this user.
            RevokeUserAuthSessions(user);

            // 3) Clear legacy user-level refresh token fields (kept temporarily during transition).
            user.RefreshToken = null;
            user.RefreshTokenExpiryUtc = null;

            // 4) Current behavior is still global access-token invalidation via TokenVersion.
            user.TokenVersion++;

            await _userRepository
                .SaveAsync(cancellationToken)
                .ConfigureAwait(false);

            return EnrichedResult.Success(true);
        }
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

            // 1) Resolve session by refresh-token hash (DB stores only hashes, not raw token values).
            var refreshTokenHash = HashRefreshToken(refreshToken);

            var authSession = await _userRepository.AuthSessions
                .Include(session => session.User)
                .SingleOrDefaultAsync(session => session.RefreshTokenHash == refreshTokenHash, cancellationToken)
                .ConfigureAwait(false);

            if (authSession is null)
            {
                // Unknown refresh token.
                return CreateAuthError();
            }

            if (authSession.RevokedUtc is not null || authSession.ExpiresUtc <= nowUtc)
            {
                // Known session but no longer active.
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
            UpdateAuthSession(authSession, authTokens, nowUtc);

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

            // 3) Revoke all refresh sessions and legacy refresh fields.
            RevokeUserAuthSessions(user);

            // Clear out the legacy refresh token so the caller is forced to login again
            user.RefreshToken = null;
            user.RefreshTokenExpiryUtc = null;

            // 4) Invalidate all existing access tokens.
            user.TokenVersion++;

            await _userRepository
                .SaveAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        return EnrichedResult.Success(true);
    }

    private AuthSessionEntity CreateAuthSession(UserEntity user, AuthTokens authTokens)
    {
        // Create one session record per login event.
        return new AuthSessionEntity
        {
            UserId = user.Id,
            User = user,
            RefreshTokenHash = HashRefreshToken(authTokens.RefreshToken),
            CreatedUtc = _timeProvider.GetUtcDateTimeNow(),
            ExpiresUtc = authTokens.RefreshTokenExpiryUtc,
            LastSeenUtc = _timeProvider.GetUtcDateTimeNow()
        };
    }

    private static void UpdateAuthSession(AuthSessionEntity authSession, AuthTokens authTokens, DateTime nowUtc)
    {
        // Session-level rotation: old refresh token hash is replaced by the new one.
        authSession.RefreshTokenHash = HashRefreshToken(authTokens.RefreshToken);
        authSession.ExpiresUtc = authTokens.RefreshTokenExpiryUtc;
        authSession.LastSeenUtc = nowUtc;
    }

    private void RevokeUserAuthSessions(UserEntity user)
    {
        // Current behavior revokes every active session for the user.
        // Step 2.4 will narrow logout to current-session semantics.
        var nowUtc = _timeProvider.GetUtcDateTimeNow();
        var userAuthSessions = _userRepository
            .Set<AuthSessionEntity>()
            .Where(authSession => authSession.UserId == user.Id && authSession.RevokedUtc == null);

        foreach (var authSession in userAuthSessions)
        {
            authSession.RevokedUtc = nowUtc;
            authSession.LastSeenUtc = nowUtc;
        }
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

    private static string HashRefreshToken(string refreshToken)
    {
        // Deterministic hash allows exact-match lookup while keeping raw tokens out of storage.
        var refreshTokenBytes = Encoding.UTF8.GetBytes(refreshToken);
        var refreshTokenHash = SHA256.HashData(refreshTokenBytes);

        return Convert.ToHexString(refreshTokenHash);
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
