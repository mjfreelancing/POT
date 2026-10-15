using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Concerns.Auth;
using Pot.App.Concerns.Time;
using Pot.App.Errors;
using Pot.AspNetCore.Concerns.Auth.Models;
using Pot.AspNetCore.Concerns.Auth.Services;
using Pot.Data.Entities;
using Pot.Data.Repositories.Users;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Pot.AspNetCore.Tests.Concerns.Auth.Services;

public class AuthServiceFixture : PotFixtureBase
{
    private readonly DateTime _nowUtc = new(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);

    private readonly IJwtService _jwtServiceFake;
    private readonly IAuthSessionService _authSessionServiceFake;
    private readonly IUserPasswordHasher _passwordHasherFake;
    private readonly IPersistableUserRepository _userRepositoryFake;
    private readonly ITimeProvider _timeProviderFake;

    public AuthServiceFixture()
    {
        _jwtServiceFake = Substitute.For<IJwtService>();
        _authSessionServiceFake = Substitute.For<IAuthSessionService>();
        _passwordHasherFake = Substitute.For<IUserPasswordHasher>();
        _userRepositoryFake = Substitute.For<IPersistableUserRepository>();
        _timeProviderFake = Substitute.For<ITimeProvider>();
    }

    public class LoginAsync : AuthServiceFixture
    {
        [Fact]
        public async Task Should_Fail_And_Not_Create_A_Session_When_The_Username_Is_Unknown()
        {
            var username = Create<string>();

            _userRepositoryFake
                .GetByUsernameAsync(username, Arg.Any<CancellationToken>())
                .Returns((UserEntity?)null);

            var service = CreateService();

            var result = await service.LoginAsync(username, Create<string>(), CreateLoginRequestContext(), TestContext.Current.CancellationToken);

            AssertAuthFailure(result);
            AssertNoSessionCreated();
            await AssertNoSaveAsync();
        }

        [Fact]
        public async Task Should_Fail_And_Not_Create_A_Session_When_The_User_Is_Disabled()
        {
            var user = CreateUser(UserStatus.Disabled);
            var password = Create<string>();

            _userRepositoryFake
                .GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>())
                .Returns(user);

            // The password is valid so the rejection can only come from the account status.
            _passwordHasherFake
                .IsValidPasswordHash(user, password, user.PasswordHash)
                .Returns(true);

            var service = CreateService();

            var result = await service.LoginAsync(user.Username, password, CreateLoginRequestContext(), TestContext.Current.CancellationToken);

            AssertAuthFailure(result);
            AssertNoSessionCreated();
            await AssertNoSaveAsync();
        }

        [Fact]
        public async Task Should_Fail_And_Not_Create_A_Session_When_The_Password_Is_Invalid()
        {
            var user = CreateUser();
            var password = Create<string>();

            _userRepositoryFake
                .GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>())
                .Returns(user);

            _passwordHasherFake
                .IsValidPasswordHash(user, password, user.PasswordHash)
                .Returns(false);

            var service = CreateService();

            var result = await service.LoginAsync(user.Username, password, CreateLoginRequestContext(), TestContext.Current.CancellationToken);

            AssertAuthFailure(result);
            AssertNoSessionCreated();
            await AssertNoSaveAsync();
        }

        [Fact]
        public async Task Should_Return_No_Tokens_And_Not_Create_A_Session_When_The_User_Is_Awaiting_Approval()
        {
            SetCurrentTime();

            var user = CreateUser(UserStatus.Approval);
            var password = Create<string>();

            _userRepositoryFake
                .GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>())
                .Returns(user);

            _passwordHasherFake
                .IsValidPasswordHash(user, password, user.PasswordHash)
                .Returns(true);

            var service = CreateService();

            var result = await service.LoginAsync(user.Username, password, CreateLoginRequestContext(), TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldBeNull();

            _jwtServiceFake
                .DidNotReceive()
                .CreateAccessToken(Arg.Any<UserEntity>());

            AssertNoSessionCreated();
            await AssertNoSaveAsync();

            user.LastLoggedInUtc.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Return_Tokens_And_Create_A_Session_And_Record_The_Login_When_Credentials_Are_Valid()
        {
            SetCurrentTime();

            var user = CreateUser();
            var password = Create<string>();
            var accessToken = Create<string>();
            var context = CreateLoginRequestContext();

            _userRepositoryFake
                .GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>())
                .Returns(user);

            _passwordHasherFake
                .IsValidPasswordHash(user, password, user.PasswordHash)
                .Returns(true);

            _jwtServiceFake
                .CreateAccessToken(user)
                .Returns(accessToken);

            var service = CreateService();

            var result = await service.LoginAsync(user.Username, password, context, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeTrue();

            var authTokens = result.Value.ShouldBeOfType<AuthTokens>();

            authTokens.AccessToken.ShouldBe(accessToken);
            authTokens.RefreshToken.ShouldNotBeNullOrWhiteSpace();
            authTokens.RefreshTokenExpiryUtc.Kind.ShouldBe(DateTimeKind.Utc);
            authTokens.RefreshTokenExpiryUtc.ShouldBeGreaterThan(_nowUtc);
            authTokens.RefreshTokenExpiryUtc.ShouldBeLessThanOrEqualTo(_nowUtc.AddDays(31));

            user.LastLoggedInUtc.ShouldBe(_nowUtc);

            // The session must be created for the same token pair that is returned to the caller.
            _authSessionServiceFake
                .Received(1)
                .CreateSession(user, authTokens.RefreshToken, authTokens.RefreshTokenExpiryUtc, _nowUtc, context);

            await _userRepositoryFake
                .Received(1)
                .SaveAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Enable_The_User_When_A_Pending_User_Logs_In()
        {
            SetCurrentTime();

            var user = CreateUser(UserStatus.Pending);
            var password = Create<string>();

            _userRepositoryFake
                .GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>())
                .Returns(user);

            _passwordHasherFake
                .IsValidPasswordHash(user, password, user.PasswordHash)
                .Returns(true);

            var service = CreateService();

            var result = await service.LoginAsync(user.Username, password, CreateLoginRequestContext(), TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeTrue();

            user.Status.ShouldBe(UserStatus.Enabled);

            await _userRepositoryFake
                .Received(1)
                .SaveAsync(Arg.Any<CancellationToken>());
        }
    }

    public class LogoutAsync : AuthServiceFixture
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Should_Return_No_Revocation_When_No_Refresh_Token_Is_Provided(string? refreshToken)
        {
            var service = CreateService();

            var result = await service.LogoutAsync(Guid.NewGuid(), refreshToken, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldBeFalse();

            await _authSessionServiceFake
                .DidNotReceive()
                .ResolveActiveSessionByRefreshTokenAsync(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());

            await AssertNoSaveAsync();
        }

        [Fact]
        public async Task Should_Return_No_Revocation_When_The_Refresh_Token_Does_Not_Resolve_An_Active_Session()
        {
            SetCurrentTime();

            var refreshToken = Create<string>();

            _authSessionServiceFake
                .ResolveActiveSessionByRefreshTokenAsync(refreshToken, _nowUtc, Arg.Any<CancellationToken>())
                .Returns((AuthSessionEntity?)null);

            var service = CreateService();

            var result = await service.LogoutAsync(Guid.NewGuid(), refreshToken, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldBeFalse();

            _authSessionServiceFake
                .DidNotReceive()
                .RevokeCurrentSession(Arg.Any<AuthSessionEntity>(), Arg.Any<DateTime>());

            await AssertNoSaveAsync();
        }

        [Fact]
        public async Task Should_Revoke_Only_The_Resolved_Session_Without_Incrementing_TokenVersion()
        {
            SetCurrentTime();

            var user = CreateUser();
            var authSession = CreateAuthSession(user);
            var refreshToken = Create<string>();
            var tokenVersionBeforeLogout = user.TokenVersion;

            _authSessionServiceFake
                .ResolveActiveSessionByRefreshTokenAsync(refreshToken, _nowUtc, Arg.Any<CancellationToken>())
                .Returns(authSession);

            var service = CreateService();

            var result = await service.LogoutAsync(user.RowId, refreshToken, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldBeTrue();

            _authSessionServiceFake
                .Received(1)
                .RevokeCurrentSession(authSession, _nowUtc);

            // Logout is per-session only; TokenVersion stays put so other devices keep their access tokens.
            user.TokenVersion.ShouldBe(tokenVersionBeforeLogout);

            await _userRepositoryFake
                .Received(1)
                .SaveAsync(Arg.Any<CancellationToken>());
        }
    }

    public class RefreshAsync : AuthServiceFixture
    {
        [Fact]
        public async Task Should_Fail_And_Not_Resolve_A_Session_When_The_Access_Token_Has_No_Subject_Claim()
        {
            var accessToken = Create<string>();

            _jwtServiceFake
                .GetPrincipalFromExpiredToken(accessToken)
                .Returns(CreatePrincipalWithSubject(null));

            var service = CreateService();

            var result = await service.RefreshAsync(accessToken, Create<string>(), TestContext.Current.CancellationToken);

            AssertAuthFailure(result);
            await AssertNoSessionResolvedAsync();
            await AssertNoSaveAsync();
        }

        [Fact]
        public async Task Should_Fail_And_Not_Resolve_A_Session_When_The_Access_Token_Subject_Is_Not_A_User_Identifier()
        {
            var accessToken = Create<string>();

            _jwtServiceFake
                .GetPrincipalFromExpiredToken(accessToken)
                .Returns(CreatePrincipalWithSubject("not-a-user-identifier"));

            var service = CreateService();

            var result = await service.RefreshAsync(accessToken, Create<string>(), TestContext.Current.CancellationToken);

            AssertAuthFailure(result);
            await AssertNoSessionResolvedAsync();
            await AssertNoSaveAsync();
        }

        [Fact]
        public async Task Should_Fail_When_The_Refresh_Token_Does_Not_Resolve_An_Active_Session()
        {
            SetCurrentTime();

            var accessToken = Create<string>();
            var refreshToken = Create<string>();

            _jwtServiceFake
                .GetPrincipalFromExpiredToken(accessToken)
                .Returns(CreatePrincipalWithSubject(Guid.NewGuid().ToString()));

            _authSessionServiceFake
                .ResolveActiveSessionByRefreshTokenAsync(refreshToken, _nowUtc, Arg.Any<CancellationToken>())
                .Returns((AuthSessionEntity?)null);

            var service = CreateService();

            var result = await service.RefreshAsync(accessToken, refreshToken, TestContext.Current.CancellationToken);

            AssertAuthFailure(result);
            AssertNoRefreshTokenRotated();
            await AssertNoSaveAsync();
        }

        [Fact]
        public async Task Should_Fail_When_The_Access_Token_Subject_Does_Not_Own_The_Session()
        {
            SetCurrentTime();

            var user = CreateUser();
            var authSession = CreateAuthSession(user);
            var accessToken = Create<string>();
            var refreshToken = Create<string>();

            _jwtServiceFake
                .GetPrincipalFromExpiredToken(accessToken)
                .Returns(CreatePrincipalWithSubject(Guid.NewGuid().ToString()));

            _authSessionServiceFake
                .ResolveActiveSessionByRefreshTokenAsync(refreshToken, _nowUtc, Arg.Any<CancellationToken>())
                .Returns(authSession);

            var service = CreateService();

            var result = await service.RefreshAsync(accessToken, refreshToken, TestContext.Current.CancellationToken);

            AssertAuthFailure(result);
            AssertNoRefreshTokenRotated();
            await AssertNoSaveAsync();
        }

        [Fact]
        public async Task Should_Rotate_The_Session_Tokens_When_The_Access_Token_Subject_Owns_The_Session()
        {
            SetCurrentTime();

            var user = CreateUser();
            var authSession = CreateAuthSession(user);
            var accessToken = Create<string>();
            var refreshToken = Create<string>();
            var rotatedAccessToken = Create<string>();

            _jwtServiceFake
                .GetPrincipalFromExpiredToken(accessToken)
                .Returns(CreatePrincipalWithSubject(user.RowId.ToString()));

            _jwtServiceFake
                .CreateAccessToken(user)
                .Returns(rotatedAccessToken);

            _authSessionServiceFake
                .ResolveActiveSessionByRefreshTokenAsync(refreshToken, _nowUtc, Arg.Any<CancellationToken>())
                .Returns(authSession);

            var service = CreateService();

            var result = await service.RefreshAsync(accessToken, refreshToken, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeTrue();

            var authTokens = result.Value.ShouldBeOfType<AuthTokens>();

            authTokens.AccessToken.ShouldBe(rotatedAccessToken);
            authTokens.RefreshToken.ShouldNotBeNullOrWhiteSpace();
            authTokens.RefreshToken.ShouldNotBe(refreshToken);
            authTokens.RefreshTokenExpiryUtc.Kind.ShouldBe(DateTimeKind.Utc);
            authTokens.RefreshTokenExpiryUtc.ShouldBeGreaterThan(_nowUtc);

            // Rotation happens on the session that was resolved, using the token pair returned to the caller.
            _authSessionServiceFake
                .Received(1)
                .RotateRefreshToken(authSession, authTokens.RefreshToken, authTokens.RefreshTokenExpiryUtc, _nowUtc);

            await _userRepositoryFake
                .Received(1)
                .SaveAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Rotate_The_Session_Tokens_When_No_Access_Token_Is_Provided()
        {
            SetCurrentTime();

            var user = CreateUser();
            var authSession = CreateAuthSession(user);
            var refreshToken = Create<string>();
            var rotatedAccessToken = Create<string>();

            _jwtServiceFake
                .CreateAccessToken(user)
                .Returns(rotatedAccessToken);

            _authSessionServiceFake
                .ResolveActiveSessionByRefreshTokenAsync(refreshToken, _nowUtc, Arg.Any<CancellationToken>())
                .Returns(authSession);

            var service = CreateService();

            var result = await service.RefreshAsync(null, refreshToken, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeTrue();

            var authTokens = result.Value.ShouldBeOfType<AuthTokens>();

            authTokens.AccessToken.ShouldBe(rotatedAccessToken);

            _jwtServiceFake
                .DidNotReceive()
                .GetPrincipalFromExpiredToken(Arg.Any<string>());

            await _userRepositoryFake
                .Received(1)
                .SaveAsync(Arg.Any<CancellationToken>());
        }
    }

    public class ChangePasswordAsync : AuthServiceFixture
    {
        [Fact]
        public async Task Should_Fail_And_Not_Write_When_The_User_Is_Not_In_The_Callers_Site()
        {
            var userId = Guid.NewGuid();

            _userRepositoryFake
                .GetForCurrentSiteAsync(userId, Arg.Any<CancellationToken>())
                .Returns((UserEntity?)null);

            var service = CreateService();

            var result = await service.ChangePasswordAsync(userId, Create<string>(), Create<string>(), TestContext.Current.CancellationToken);

            result.IsFail.ShouldBeTrue();

            var error = result.Error.ShouldBeOfType<ApiDetailError>();

            error.ErrorType.ShouldBe(ErrorType.NotFound);
            error.AttemptedValue.ShouldBe(userId);
            error.ErrorMessage.ShouldBe("The user does not exist");

            _passwordHasherFake
                .DidNotReceive()
                .IsValidPasswordHash(Arg.Any<UserEntity>(), Arg.Any<string>(), Arg.Any<string>());

            await _authSessionServiceFake
                .DidNotReceive()
                .RevokeAllSessionsForUserAsync(Arg.Any<int>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());

            await AssertNoSaveAsync();
        }

        [Fact]
        public async Task Should_Change_The_Password_And_Revoke_All_Sessions_When_The_Current_Password_Is_Valid()
        {
            SetCurrentTime();

            var user = CreateUser();
            var currentPassword = Create<string>();
            var newPassword = Create<string>();
            var newPasswordHash = Create<string>();
            var tokenVersionBeforeChange = user.TokenVersion;

            _userRepositoryFake
                .GetForCurrentSiteAsync(user.RowId, Arg.Any<CancellationToken>())
                .Returns(user);

            _passwordHasherFake
                .IsValidPasswordHash(user, currentPassword, user.PasswordHash)
                .Returns(true);

            _passwordHasherFake
                .GetHash(user, newPassword)
                .Returns(newPasswordHash);

            var service = CreateService();

            var result = await service.ChangePasswordAsync(user.RowId, currentPassword, newPassword, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldBeTrue();

            user.PasswordHash.ShouldBe(newPasswordHash);
            user.TokenVersion.ShouldBe(tokenVersionBeforeChange + 1);

            await _authSessionServiceFake
                .Received(1)
                .RevokeAllSessionsForUserAsync(user.Id, _nowUtc, Arg.Any<CancellationToken>());

            await _userRepositoryFake
                .Received(1)
                .SaveAsync(Arg.Any<CancellationToken>());
        }
    }

    private AuthService CreateService()
    {
        var logger = Substitute.For<ILogger<AuthService>>();

        return new AuthService(_jwtServiceFake, _authSessionServiceFake, _passwordHasherFake, _userRepositoryFake,
            _timeProviderFake, logger);
    }

    private void SetCurrentTime()
    {
        _timeProviderFake
            .GetUtcDateTimeNow()
            .Returns(_nowUtc);

        _timeProviderFake
            .GetLocalDateNow()
            .Returns(DateOnly.FromDateTime(_nowUtc));
    }

    private UserEntity CreateUser(UserStatus? status = null)
    {
        var user = EntityFactory.CreateUser(EntityFactory.CreateSite(), "auth-service-user");

        user.Id = Create<int>();
        user.Status = status ?? UserStatus.Enabled;

        return user;
    }

    private AuthSessionEntity CreateAuthSession(UserEntity user)
    {
        return new AuthSessionEntity
        {
            RowId = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            RefreshTokenHash = Create<string>(),
            CreatedUtc = _nowUtc.AddDays(-1),
            ExpiresUtc = _nowUtc.AddDays(30),
            LastSeenUtc = _nowUtc
        };
    }

    private static LoginRequestContext CreateLoginRequestContext()
    {
        return new LoginRequestContext("POT AuthService Unit Test Agent/1.0", "203.0.113.10");
    }

    private static ClaimsPrincipal CreatePrincipalWithSubject(string? subject)
    {
        var identity = subject is null
            ? new ClaimsIdentity()
            : new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, subject)]);

        return new ClaimsPrincipal(identity);
    }

    private static void AssertAuthFailure(EnrichedResult<AuthTokens?> result)
    {
        result.IsFail.ShouldBeTrue();

        var error = result.Error.ShouldBeOfType<ApiDetailError>();

        error.ErrorType.ShouldBe(ErrorType.Auth);
        error.ErrorMessage.ShouldBe("Invalid username or password");
    }

    private void AssertNoSessionCreated()
    {
        _authSessionServiceFake
            .DidNotReceive()
            .CreateSession(Arg.Any<UserEntity>(), Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<LoginRequestContext>());
    }

    private void AssertNoRefreshTokenRotated()
    {
        _authSessionServiceFake
            .DidNotReceive()
            .RotateRefreshToken(Arg.Any<AuthSessionEntity>(), Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<DateTime>());
    }

    private async Task AssertNoSessionResolvedAsync()
    {
        await _authSessionServiceFake
            .DidNotReceive()
            .ResolveActiveSessionByRefreshTokenAsync(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    private async Task AssertNoSaveAsync()
    {
        await _userRepositoryFake
            .DidNotReceive()
            .SaveAsync(Arg.Any<CancellationToken>());
    }
}
