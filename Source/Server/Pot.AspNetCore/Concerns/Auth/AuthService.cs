using AllOverIt.Assertion;
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

namespace Pot.AspNetCore.Concerns.Auth;

internal sealed class AuthService : IAuthService
{
    private sealed class UserIsNotDisabledSpecification : Specification<UserEntity?>
    {
        public override bool IsSatisfiedBy(UserEntity? candidate)
        {
            var status = candidate?.Status;

            return status is not null && status != Shared.Enumerations.UserStatus.Disabled;
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

    private static readonly UserIsNotDisabledSpecification _userIsNotDisabledSpecification = new();

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

            if (user.Status == UserStatus.Pending)
            {
                user.Status = UserStatus.Enabled;
            }

            var authTokens = SetUserAuthTokens(user);

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
            var user = await _userRepository.Users
                .SingleOrDefaultAsync(user => user.RowId == userId, cancellationToken)
                .ConfigureAwait(false);

            if (user is null)
            {
                // Not doing anything with this result - merely indicating the user is not logged out since not found)
                return EnrichedResult.Success(false);
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiryUtc = null;

            await _userRepository
                .SaveAsync(cancellationToken)
                .ConfigureAwait(false);

            return EnrichedResult.Success(true);
        }
    }

    public async Task<EnrichedResult<AuthTokens?>> RefreshAsync(string accessToken, string refreshToken, CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { refreshToken });

        var principal = _jwtService.GetPrincipalFromExpiredToken(accessToken);

        var subject = principal.Claims.SingleOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub);

        if (subject is null || !Guid.TryParse(subject.Value, out var userRowId))
        {
            return CreateAuthError();
        }

        using (_userRepository.WithTracking())
        {
            var user = await _userRepository.Users
                .SingleOrDefaultAsync(user => user.RowId == userRowId, cancellationToken)
                .ConfigureAwait(false);

            if (user is null ||
                user.RefreshToken != refreshToken ||
                user.RefreshTokenExpiryUtc <= _timeProvider.GetUtcDateTimeNow())
            {
                return CreateAuthError();
            }

            var authTokens = SetUserAuthTokens(user);

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
            var user = await _userRepository.Users
                .SingleOrDefaultAsync(user => user.RowId == userId, cancellationToken)
                .ConfigureAwait(false);

            Throw<UnreachableException>.WhenNull(user, "The user should exist");

            var isValidHash = _passwordHasher.IsValidPasswordHash(user, currentPassword, user.PasswordHash);

            if (!isValidHash)
            {
                var authError = ProblemDetailsErrorFactory.CreateAuthError("The current password is invalid.");
                return EnrichedResult.Fail<bool>(authError);
            }

            user.PasswordHash = _passwordHasher.GetHash(user, newPassword);

            await _userRepository
                .SaveAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        return EnrichedResult.Success(true);
    }

    private AuthTokens SetUserAuthTokens(UserEntity user)
    {
        // This includes generating a new refresh token
        var authTokens = CreateUserAuthTokens(user);

        user.RefreshToken = authTokens.RefreshToken;
        user.RefreshTokenExpiryUtc = authTokens.RefreshTokenExpiryUtc;

        return authTokens;
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
        var loginProblem = ProblemDetailsErrorFactory.CreateAuthError("Invalid username or password");

        return EnrichedResult.Fail<AuthTokens?>(loginProblem);
    }

    private static ISpecification<UserEntity?> CreateValidUserSpecification(IUserPasswordHasher passwordHasher, string password)
    {
        return _userIsNotDisabledSpecification.And(new UserProvidedCorrectPasswordSpecification(passwordHasher, password));
    }
}
