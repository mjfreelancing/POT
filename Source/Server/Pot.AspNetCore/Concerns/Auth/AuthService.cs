using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Pot.App.Concerns.Time;
using Pot.App.Errors;
using Pot.AspNetCore.Concerns.Auth.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Users;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace Pot.AspNetCore.Concerns.Auth;

internal sealed class AuthService : IAuthService
{
    private const int RefreshTokenExpiryDays = 30;

    private readonly IPersistableUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IUserPasswordHasher _passwordHasher;
    private readonly ITimeProvider _timeProvider;
    private readonly ILogger _logger;

    public AuthService(IJwtService jwtProvider, IUserPasswordHasher passwordHasher, IPersistableUserRepository userRepository,
        ITimeProvider timeProvider, ILogger<AuthService> logger)
    {
        _userRepository = userRepository.WhenNotNull();
        _jwtService = jwtProvider.WhenNotNull();
        _passwordHasher = passwordHasher.WhenNotNull();
        _timeProvider = timeProvider.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<AuthTokens?>> LoginAsync(string username, string password, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        using (_userRepository.WithTracking())
        {
            var user = await _userRepository
                .SingleOrDefaultAsync(user => user.Username == username, cancellationToken)
                .ConfigureAwait(false);

            if (user is null)
            {
                return CreateAuthError();
            }

            var isValidHash = _passwordHasher.IsValidPasswordHash(user, password, user.PasswordHash);

            if (!isValidHash)
            {
                return CreateAuthError();
            }

            var authTokens = await UpdateUserAuthTokensAsync(user, cancellationToken).ConfigureAwait(false);

            return EnrichedResult.Success<AuthTokens?>(authTokens);
        }
    }

    public async Task<EnrichedResult<AuthTokens?>> RefreshAsync(string accessToken, string refreshToken, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var principal = _jwtService.GetPrincipalFromExpiredToken(accessToken);

        var subject = principal.Claims.SingleOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub);

        if (subject is null || !Guid.TryParse(subject.Value, out var userRowId))
        {
            return CreateAuthError();
        }

        using (_userRepository.WithTracking())
        {
            var user = await _userRepository
                .SingleOrDefaultAsync(user => user.RowId == userRowId, cancellationToken)
                .ConfigureAwait(false);

            if (user is null ||
                user.RefreshToken != refreshToken ||
                user.RefreshTokenExpiryUtc <= _timeProvider.GetUtcDateTimeNow())
            {
                return CreateAuthError();
            }

            var authTokens = await UpdateUserAuthTokensAsync(user, cancellationToken).ConfigureAwait(false);

            return EnrichedResult.Success<AuthTokens?>(authTokens);
        }
    }

    private async Task<AuthTokens> UpdateUserAuthTokensAsync(UserEntity user, CancellationToken cancellationToken)
    {
        var authTokens = CreateUserAuthTokens(user);

        user.RefreshToken = authTokens.RefreshToken;
        user.RefreshTokenExpiryUtc = authTokens.RefreshTokenExpiryUtc;

        await _userRepository.SaveAsync(cancellationToken).ConfigureAwait(false);

        return authTokens;
    }

    private AuthTokens CreateUserAuthTokens(UserEntity user)
    {
        var accessToken = _jwtService.CreateAccessToken(user);
        var refreshToken = GenerateRefreshToken();
        var refreshTokenExpiry = _timeProvider.GetUtcDateTimeNow().AddDays(RefreshTokenExpiryDays);

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
        // This also applies to attempting to refresh an access token with a refresh token that has expired.
        var loginProblem = ProblemDetailsErrorFactory.CreateAuthError("The username or password is invalid.");

        return EnrichedResult.Fail<AuthTokens?>(loginProblem);
    }
}
