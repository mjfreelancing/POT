using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.AspNetCore.Concerns.Auth.Models;
using Pot.AspNetCore.Concerns.Auth.Services;
using Pot.Data.Entities;
using Pot.Data.Repositories.AuthSessions;
using Pot.TestUtils;
using Shouldly;

namespace Pot.AspNetCore.Tests.Concerns.Auth.Services;

public class AuthSessionServiceFixture : PotFixtureBase
{
    private static readonly LoginRequestContext NoLoginRequestContext = new(null, null);
    public class Constructor : AuthSessionServiceFixture
    {
        private readonly IPersistableAuthSessionRepository _authSessionRepositoryFake;
        private readonly ILogger<AuthSessionService> _loggerFake;

        public Constructor()
        {
            _authSessionRepositoryFake = Substitute.For<IPersistableAuthSessionRepository>();
            _loggerFake = Substitute.For<ILogger<AuthSessionService>>();
        }

        [Fact]
        public void Should_Throw_When_AuthSessionRepository_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                _ = new AuthSessionService(null!, _loggerFake);
            });

            exception.ParamName.ShouldBe("authSessionRepository");
        }

        [Fact]
        public void Should_Throw_When_Logger_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                _ = new AuthSessionService(_authSessionRepositoryFake, null!);
            });

            exception.ParamName.ShouldBe("logger");
        }
    }

    public class CreateSession : AuthSessionServiceFixture
    {
        private readonly IPersistableAuthSessionRepository _authSessionRepositoryFake;

        public CreateSession()
        {
            _authSessionRepositoryFake = Substitute.For<IPersistableAuthSessionRepository>();
        }

        [Fact]
        public void Should_Return_Session_With_Correct_Field_Values()
        {
            var logger = Substitute.For<ILogger<AuthSessionService>>();
            var service = new AuthSessionService(_authSessionRepositoryFake, logger);

            var user = CreateUserWithId(userId: 7);
            var nowUtc = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);
            var expiryUtc = nowUtc.AddDays(30);
            var refreshToken = Create<string>();

            var authSession = service.CreateSession(user, refreshToken, expiryUtc, nowUtc, NoLoginRequestContext);

            authSession.UserId.ShouldBe(user.Id);
            authSession.User.ShouldBe(user);
            authSession.CreatedUtc.ShouldBe(nowUtc);
            authSession.ExpiresUtc.ShouldBe(expiryUtc);
            authSession.LastSeenUtc.ShouldBe(nowUtc);
        }

        [Fact]
        public void Should_Store_Hashed_RefreshToken_Not_Raw_Token()
        {
            var logger = Substitute.For<ILogger<AuthSessionService>>();
            var service = new AuthSessionService(_authSessionRepositoryFake, logger);

            var user = CreateUserWithId(userId: 3);
            var nowUtc = DateTime.UtcNow;
            var refreshToken = Create<string>();

            var authSession = service.CreateSession(user, refreshToken, nowUtc.AddDays(7), nowUtc, NoLoginRequestContext);

            authSession.RefreshTokenHash.ShouldNotBe(refreshToken);
            authSession.RefreshTokenHash.ShouldNotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Should_Produce_Consistent_Hash_For_Same_RefreshToken()
        {
            var logger = Substitute.For<ILogger<AuthSessionService>>();
            var service = new AuthSessionService(_authSessionRepositoryFake, logger);

            var user = CreateUserWithId(userId: 5);
            var nowUtc = DateTime.UtcNow;
            var refreshToken = Create<string>();

            var firstSession = service.CreateSession(user, refreshToken, nowUtc.AddDays(7), nowUtc, NoLoginRequestContext);
            var secondSession = service.CreateSession(user, refreshToken, nowUtc.AddDays(7), nowUtc, NoLoginRequestContext);

            firstSession.RefreshTokenHash.ShouldBe(secondSession.RefreshTokenHash);
        }

        [Fact]
        public void Should_Add_Session_To_Repository()
        {
            var logger = Substitute.For<ILogger<AuthSessionService>>();
            var service = new AuthSessionService(_authSessionRepositoryFake, logger);

            var user = CreateUserWithId(userId: 9);
            var nowUtc = DateTime.UtcNow;

            var authSession = service.CreateSession(user, Create<string>(), nowUtc.AddDays(7), nowUtc, NoLoginRequestContext);

            _authSessionRepositoryFake.Received(1).Add(authSession);
        }

        [Fact]
        public void Should_Store_UserAgent_And_IpAddress_On_Session()
        {
            var logger = Substitute.For<ILogger<AuthSessionService>>();
            var service = new AuthSessionService(_authSessionRepositoryFake, logger);

            var user = CreateUserWithId(userId: 13);
            var nowUtc = DateTime.UtcNow;

            var authSession = service.CreateSession(user, Create<string>(), nowUtc.AddDays(7), nowUtc,
                new LoginRequestContext(UserAgent: "Mozilla/5.0 POT Test", IpAddress: "203.0.113.10"));

            authSession.UserAgent.ShouldBe("Mozilla/5.0 POT Test");
            authSession.IpAddress.ShouldBe("203.0.113.10");
        }

        /*
        TODO(logging): Re-enable when the replacement logging test framework is available.
        [Fact]
        public void Should_LogCall_When_Creating_Session()
        {
            var logger = Substitute.For<ILogger<AuthSessionService>>();
            var service = new AuthSessionService(_authSessionRepositoryFake, logger);

            var user = CreateUserWithId(userId: 7);
            var nowUtc = DateTime.UtcNow;

            var context = logger.CaptureLogCalls(() =>
            {
                _ = service.CreateSession(user, Create<string>(), nowUtc.AddDays(7), nowUtc);
            });

            _ = context
                .ShouldLogCallAndArgs<AuthSessionService>(nameof(AuthSessionService.CreateSession))
                .ShouldHaveArgsEquivalentTo(new { userId = user.Id });
        }
        */
    }

    public class ResolveActiveSessionByRefreshTokenAsync : AuthSessionServiceFixture
    {
        private readonly IPersistableAuthSessionRepository _authSessionRepositoryFake;

        public ResolveActiveSessionByRefreshTokenAsync()
        {
            _authSessionRepositoryFake = Substitute.For<IPersistableAuthSessionRepository>();
        }

        [Fact]
        public async Task Should_Return_Null_When_Session_Not_Found()
        {
            var logger = Substitute.For<ILogger<AuthSessionService>>();
            var service = new AuthSessionService(_authSessionRepositoryFake, logger);

            _authSessionRepositoryFake
                .GetByRefreshTokenHashOrDefaultAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns((AuthSessionEntity?)null);

            var result = await service.ResolveActiveSessionByRefreshTokenAsync(Create<string>(), DateTime.UtcNow, CancellationToken.None);

            result.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Return_Null_When_Session_Is_Revoked()
        {
            var logger = Substitute.For<ILogger<AuthSessionService>>();
            var service = new AuthSessionService(_authSessionRepositoryFake, logger);

            var nowUtc = DateTime.UtcNow;
            var revokedSession = CreateActiveSession(expiresUtc: nowUtc.AddDays(7));
            revokedSession.RevokedUtc = nowUtc.AddMinutes(-5);

            _authSessionRepositoryFake
                .GetByRefreshTokenHashOrDefaultAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(revokedSession);

            var result = await service.ResolveActiveSessionByRefreshTokenAsync(Create<string>(), nowUtc, CancellationToken.None);

            result.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Return_Null_When_Session_Has_Expired()
        {
            var logger = Substitute.For<ILogger<AuthSessionService>>();
            var service = new AuthSessionService(_authSessionRepositoryFake, logger);

            var nowUtc = DateTime.UtcNow;
            var expiredSession = CreateActiveSession(expiresUtc: nowUtc.AddSeconds(-1));

            _authSessionRepositoryFake
                .GetByRefreshTokenHashOrDefaultAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(expiredSession);

            var result = await service.ResolveActiveSessionByRefreshTokenAsync(Create<string>(), nowUtc, CancellationToken.None);

            result.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Return_Null_When_Session_Expires_Exactly_At_NowUtc()
        {
            var logger = Substitute.For<ILogger<AuthSessionService>>();
            var service = new AuthSessionService(_authSessionRepositoryFake, logger);

            var nowUtc = DateTime.UtcNow;
            var exactlyExpiredSession = CreateActiveSession(expiresUtc: nowUtc);

            _authSessionRepositoryFake
                .GetByRefreshTokenHashOrDefaultAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(exactlyExpiredSession);

            var result = await service.ResolveActiveSessionByRefreshTokenAsync(Create<string>(), nowUtc, CancellationToken.None);

            result.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Return_Session_When_Session_Is_Active()
        {
            var logger = Substitute.For<ILogger<AuthSessionService>>();
            var service = new AuthSessionService(_authSessionRepositoryFake, logger);

            var nowUtc = DateTime.UtcNow;
            var activeSession = CreateActiveSession(expiresUtc: nowUtc.AddDays(7));

            _authSessionRepositoryFake
                .GetByRefreshTokenHashOrDefaultAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(activeSession);

            var result = await service.ResolveActiveSessionByRefreshTokenAsync(Create<string>(), nowUtc, CancellationToken.None);

            result.ShouldBe(activeSession);
        }

        [Fact]
        public async Task Should_Query_Repository_With_Hashed_Token_Not_Raw_Token()
        {
            var logger = Substitute.For<ILogger<AuthSessionService>>();
            var service = new AuthSessionService(_authSessionRepositoryFake, logger);

            var rawRefreshToken = Create<string>();
            string? capturedHash = null;

            _authSessionRepositoryFake
                .GetByRefreshTokenHashOrDefaultAsync(
                    Arg.Do<string>(hash => capturedHash = hash),
                    Arg.Any<bool>(),
                    Arg.Any<CancellationToken>())
                .Returns((AuthSessionEntity?)null);

            _ = await service.ResolveActiveSessionByRefreshTokenAsync(rawRefreshToken, DateTime.UtcNow, CancellationToken.None);

            capturedHash.ShouldNotBeNull();
            capturedHash.ShouldNotBe(rawRefreshToken);
        }

        [Fact]
        public async Task Should_Request_User_Include_When_Querying_Repository()
        {
            var logger = Substitute.For<ILogger<AuthSessionService>>();
            var service = new AuthSessionService(_authSessionRepositoryFake, logger);

            bool? capturedIncludeUser = null;

            _authSessionRepositoryFake
                .GetByRefreshTokenHashOrDefaultAsync(
                    Arg.Any<string>(),
                    Arg.Do<bool>(includeUser => capturedIncludeUser = includeUser),
                    Arg.Any<CancellationToken>())
                .Returns((AuthSessionEntity?)null);

            _ = await service.ResolveActiveSessionByRefreshTokenAsync(Create<string>(), DateTime.UtcNow, CancellationToken.None);

            capturedIncludeUser.ShouldBe(true);
        }

        /*
        TODO(logging): Re-enable when the replacement logging test framework is available.
        [Fact]
        public async Task Should_LogCall_When_Resolving_Session_By_Refresh_Token()
        {
            var logger = Substitute.For<ILogger<AuthSessionService>>();
            var service = new AuthSessionService(_authSessionRepositoryFake, logger);

            _authSessionRepositoryFake
                .GetByRefreshTokenHashOrDefaultAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns((AuthSessionEntity?)null);

            var context = await logger.CaptureLogCallsAsync(async () =>
            {
                _ = await service.ResolveActiveSessionByRefreshTokenAsync(Create<string>(), DateTime.UtcNow, CancellationToken.None);
            });

            _ = context.ShouldLogCall<AuthSessionService>(nameof(AuthSessionService.ResolveActiveSessionByRefreshTokenAsync));
        }
        */
    }

    public class RotateRefreshToken : AuthSessionServiceFixture
    {
        private readonly IPersistableAuthSessionRepository _authSessionRepositoryFake;

        public RotateRefreshToken()
        {
            _authSessionRepositoryFake = Substitute.For<IPersistableAuthSessionRepository>();
        }

        [Fact]
        public void Should_Update_RefreshTokenHash_To_New_Hash()
        {
            var logger = Substitute.For<ILogger<AuthSessionService>>();
            var service = new AuthSessionService(_authSessionRepositoryFake, logger);

            var authSession = CreateActiveSession();
            var previousHash = authSession.RefreshTokenHash;
            var newRefreshToken = Create<string>();
            var nowUtc = DateTime.UtcNow;

            service.RotateRefreshToken(authSession, newRefreshToken, nowUtc.AddDays(7), nowUtc);

            authSession.RefreshTokenHash.ShouldNotBe(previousHash);
            authSession.RefreshTokenHash.ShouldNotBe(newRefreshToken);
            authSession.RefreshTokenHash.ShouldNotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Should_Update_ExpiresUtc_When_Rotating_Token()
        {
            var logger = Substitute.For<ILogger<AuthSessionService>>();
            var service = new AuthSessionService(_authSessionRepositoryFake, logger);

            var authSession = CreateActiveSession();
            var nowUtc = DateTime.UtcNow;
            var newExpiryUtc = nowUtc.AddDays(14);

            service.RotateRefreshToken(authSession, Create<string>(), newExpiryUtc, nowUtc);

            authSession.ExpiresUtc.ShouldBe(newExpiryUtc);
        }

        [Fact]
        public void Should_Update_LastSeenUtc_To_NowUtc_When_Rotating_Token()
        {
            var logger = Substitute.For<ILogger<AuthSessionService>>();
            var service = new AuthSessionService(_authSessionRepositoryFake, logger);

            var authSession = CreateActiveSession();
            var nowUtc = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);

            service.RotateRefreshToken(authSession, Create<string>(), nowUtc.AddDays(7), nowUtc);

            authSession.LastSeenUtc.ShouldBe(nowUtc);
        }

        /*
        TODO(logging): Re-enable when the replacement logging test framework is available.
        [Fact]
        public void Should_LogCall_When_Rotating_Refresh_Token()
        {
            var logger = Substitute.For<ILogger<AuthSessionService>>();
            var service = new AuthSessionService(_authSessionRepositoryFake, logger);

            var knownRowId = Guid.NewGuid();
            var authSession = CreateActiveSession(rowId: knownRowId);
            var nowUtc = DateTime.UtcNow;

            var context = logger.CaptureLogCalls(() =>
            {
                service.RotateRefreshToken(authSession, Create<string>(), nowUtc.AddDays(7), nowUtc);
            });

            _ = context
                .ShouldLogCallAndArgs<AuthSessionService>(nameof(AuthSessionService.RotateRefreshToken))
                .ShouldHaveArgsEquivalentTo(new { sessionRowId = knownRowId });
        }
        */
    }

    public class RevokeCurrentSession : AuthSessionServiceFixture
    {
        private readonly IPersistableAuthSessionRepository _authSessionRepositoryFake;

        public RevokeCurrentSession()
        {
            _authSessionRepositoryFake = Substitute.For<IPersistableAuthSessionRepository>();
        }

        [Fact]
        public void Should_Set_RevokedUtc_To_NowUtc_When_Revoking()
        {
            var logger = Substitute.For<ILogger<AuthSessionService>>();
            var service = new AuthSessionService(_authSessionRepositoryFake, logger);

            var authSession = CreateActiveSession();
            var nowUtc = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);

            service.RevokeCurrentSession(authSession, nowUtc);

            authSession.RevokedUtc.ShouldBe(nowUtc);
        }

        [Fact]
        public void Should_Set_LastSeenUtc_To_NowUtc_When_Revoking()
        {
            var logger = Substitute.For<ILogger<AuthSessionService>>();
            var service = new AuthSessionService(_authSessionRepositoryFake, logger);

            var authSession = CreateActiveSession();
            var nowUtc = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);

            service.RevokeCurrentSession(authSession, nowUtc);

            authSession.LastSeenUtc.ShouldBe(nowUtc);
        }

        /*
        TODO(logging): Re-enable when the replacement logging test framework is available.
        [Fact]
        public void Should_LogCall_When_Revoking_Current_Session()
        {
            var logger = Substitute.For<ILogger<AuthSessionService>>();
            var service = new AuthSessionService(_authSessionRepositoryFake, logger);

            var knownRowId = Guid.NewGuid();
            var authSession = CreateActiveSession(rowId: knownRowId);
            var nowUtc = DateTime.UtcNow;

            var context = logger.CaptureLogCalls(() =>
            {
                service.RevokeCurrentSession(authSession, nowUtc);
            });

            _ = context
                .ShouldLogCallAndArgs<AuthSessionService>(nameof(AuthSessionService.RevokeCurrentSession))
                .ShouldHaveArgsEquivalentTo(new { sessionRowId = knownRowId });
        }
        */
    }

    public class RevokeAllSessionsForUserAsync : AuthSessionServiceFixture
    {
        private readonly IPersistableAuthSessionRepository _authSessionRepositoryFake;

        public RevokeAllSessionsForUserAsync()
        {
            _authSessionRepositoryFake = Substitute.For<IPersistableAuthSessionRepository>();
        }

        [Fact]
        public async Task Should_Return_Zero_When_No_Sessions_Exist_For_User()
        {
            var logger = Substitute.For<ILogger<AuthSessionService>>();
            var service = new AuthSessionService(_authSessionRepositoryFake, logger);

            _authSessionRepositoryFake
                .GetUnrevokedByUserIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new List<AuthSessionEntity>()));

            var count = await service.RevokeAllSessionsForUserAsync(1, DateTime.UtcNow, CancellationToken.None);

            count.ShouldBe(0);
        }

        [Fact]
        public async Task Should_Return_Count_Of_Sessions_Revoked()
        {
            var logger = Substitute.For<ILogger<AuthSessionService>>();
            var service = new AuthSessionService(_authSessionRepositoryFake, logger);

            var nowUtc = DateTime.UtcNow;
            var sessions = new List<AuthSessionEntity>
            {
                CreateActiveSession(expiresUtc: nowUtc.AddDays(7)),
                CreateActiveSession(expiresUtc: nowUtc.AddDays(14)),
                CreateActiveSession(expiresUtc: nowUtc.AddDays(3))
            };

            _authSessionRepositoryFake
                .GetUnrevokedByUserIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(sessions));

            var count = await service.RevokeAllSessionsForUserAsync(1, nowUtc, CancellationToken.None);

            count.ShouldBe(3);
        }

        [Fact]
        public async Task Should_Set_RevokedUtc_On_Each_Session()
        {
            var logger = Substitute.For<ILogger<AuthSessionService>>();
            var service = new AuthSessionService(_authSessionRepositoryFake, logger);

            var nowUtc = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);
            var sessionA = CreateActiveSession(expiresUtc: nowUtc.AddDays(7));
            var sessionB = CreateActiveSession(expiresUtc: nowUtc.AddDays(3));

            _authSessionRepositoryFake
                .GetUnrevokedByUserIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new List<AuthSessionEntity> { sessionA, sessionB }));

            _ = await service.RevokeAllSessionsForUserAsync(1, nowUtc, CancellationToken.None);

            sessionA.RevokedUtc.ShouldBe(nowUtc);
            sessionB.RevokedUtc.ShouldBe(nowUtc);
        }

        [Fact]
        public async Task Should_Set_LastSeenUtc_On_Each_Session()
        {
            var logger = Substitute.For<ILogger<AuthSessionService>>();
            var service = new AuthSessionService(_authSessionRepositoryFake, logger);

            var nowUtc = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);
            var sessionA = CreateActiveSession(expiresUtc: nowUtc.AddDays(7));
            var sessionB = CreateActiveSession(expiresUtc: nowUtc.AddDays(3));

            _authSessionRepositoryFake
                .GetUnrevokedByUserIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new List<AuthSessionEntity> { sessionA, sessionB }));

            _ = await service.RevokeAllSessionsForUserAsync(1, nowUtc, CancellationToken.None);

            sessionA.LastSeenUtc.ShouldBe(nowUtc);
            sessionB.LastSeenUtc.ShouldBe(nowUtc);
        }

        [Fact]
        public async Task Should_Query_Repository_With_Correct_UserId()
        {
            var logger = Substitute.For<ILogger<AuthSessionService>>();
            var service = new AuthSessionService(_authSessionRepositoryFake, logger);

            var userId = 99;

            _authSessionRepositoryFake
                .GetUnrevokedByUserIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new List<AuthSessionEntity>()));

            _ = await service.RevokeAllSessionsForUserAsync(userId, DateTime.UtcNow, CancellationToken.None);

            await _authSessionRepositoryFake
                .Received(1)
                .GetUnrevokedByUserIdAsync(userId, Arg.Any<CancellationToken>());
        }

        /*
        TODO(logging): Re-enable when the replacement logging test framework is available.
        [Fact]
        public async Task Should_LogCall_When_Revoking_All_Sessions_For_User()
        {
            var logger = Substitute.For<ILogger<AuthSessionService>>();
            var service = new AuthSessionService(_authSessionRepositoryFake, logger);

            const int userId = 42;

            _authSessionRepositoryFake
                .GetUnrevokedByUserIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new List<AuthSessionEntity>()));

            var context = await logger.CaptureLogCallsAsync(async () =>
            {
                _ = await service.RevokeAllSessionsForUserAsync(userId, DateTime.UtcNow, CancellationToken.None);
            });

            _ = context
                .ShouldLogCallAndArgs<AuthSessionService>(nameof(AuthSessionService.RevokeAllSessionsForUserAsync))
                .ShouldHaveArgsEquivalentTo(new { userId });
        }
        */
    }

    private static UserEntity CreateUserWithId(int userId)
    {
        var site = EntityFactory.CreateSite();
        var user = EntityFactory.CreateUser(site);
        user.Id = userId;

        return user;
    }

    private static AuthSessionEntity CreateActiveSession(DateTime? expiresUtc = null, Guid? rowId = null)
    {
        var nowUtc = DateTime.UtcNow;

        var session = new AuthSessionEntity
        {
            RefreshTokenHash = Guid.NewGuid().ToString("N"),
            CreatedUtc = nowUtc,
            ExpiresUtc = expiresUtc ?? nowUtc.AddDays(7),
            LastSeenUtc = nowUtc
        };

        if (rowId.HasValue)
        {
            session.RowId = rowId.Value;
        }

        return session;
    }
}
