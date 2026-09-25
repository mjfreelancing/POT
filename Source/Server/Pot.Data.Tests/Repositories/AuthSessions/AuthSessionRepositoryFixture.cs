using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Pot.Data.Entities;
using Pot.Data.Repositories.AuthSessions;
using Pot.Shared;
using Pot.TestUtils;
using Shouldly;

namespace Pot.Data.Tests.Repositories.AuthSessions;

public class AuthSessionRepositoryFixture : PotFixtureBase
{
    private sealed class TestContext : IDisposable
    {
        public PotDbContext DbContext { get; }
        public AuthSessionRepository Repository { get; }
        public SiteEntity Site { get; }
        public UserEntity User { get; }
        public UserEntity OtherUser { get; }

        public TestContext(PotDbContext dbContext, AuthSessionRepository repository, SiteEntity site, UserEntity user, UserEntity otherUser)
        {
            DbContext = dbContext;
            Repository = repository;
            Site = site;
            User = user;
            OtherUser = otherUser;
        }

        public Task<int> AddSessionsAsync(params AuthSessionEntity[] sessions)
        {
            foreach (var session in sessions)
            {
                DbContext.Add(session);
            }

            return DbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }

    public class GetByRefreshTokenHashOrDefaultAsync : AuthSessionRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_The_Session_When_The_Hash_Matches()
        {
            using var context = CreateTestContext();

            var session = CreateSession(context.User, "matching-hash");
            await context.AddSessionsAsync(session);

            var result = await context.Repository.GetByRefreshTokenHashOrDefaultAsync("matching-hash", includeUser: false, Xunit.TestContext.Current.CancellationToken);

            result.ShouldNotBeNull();
            result.RowId.ShouldBe(session.RowId);
        }

        [Fact]
        public async Task Should_Return_Null_When_The_Hash_Does_Not_Match()
        {
            using var context = CreateTestContext();

            var session = CreateSession(context.User, "matching-hash");
            await context.AddSessionsAsync(session);

            var result = await context.Repository.GetByRefreshTokenHashOrDefaultAsync("unknown-hash", includeUser: false, Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Not_Load_The_User_When_Not_Requested()
        {
            using var context = CreateTestContext();

            var session = CreateSession(context.User, "matching-hash");
            await context.AddSessionsAsync(session);

            var result = await context.Repository.GetByRefreshTokenHashOrDefaultAsync("matching-hash", includeUser: false, Xunit.TestContext.Current.CancellationToken);

            result.ShouldNotBeNull();
            result.User.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Load_The_User_When_Requested()
        {
            using var context = CreateTestContext();

            var session = CreateSession(context.User, "matching-hash");
            await context.AddSessionsAsync(session);

            var result = await context.Repository.GetByRefreshTokenHashOrDefaultAsync("matching-hash", includeUser: true, Xunit.TestContext.Current.CancellationToken);

            result.ShouldNotBeNull();
            result.User.ShouldNotBeNull();
            result.User.RowId.ShouldBe(context.User.RowId);
        }
    }

    public class GetUnrevokedByUserIdAsync : AuthSessionRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_Only_Unrevoked_Sessions_For_The_User()
        {
            using var context = CreateTestContext();

            var activeSession = CreateSession(context.User, "active-hash");
            var revokedSession = CreateSession(context.User, "revoked-hash");
            revokedSession.RevokedUtc = DateTime.UtcNow;

            var otherUserSession = CreateSession(context.OtherUser, "other-user-hash");

            await context.AddSessionsAsync(activeSession, revokedSession, otherUserSession);

            var result = await context.Repository.GetUnrevokedByUserIdAsync(context.User.Id, Xunit.TestContext.Current.CancellationToken);

            result.Count.ShouldBe(1);
            result[0].RowId.ShouldBe(activeSession.RowId);
        }

        [Fact]
        public async Task Should_Return_Empty_List_When_The_User_Has_No_Unrevoked_Sessions()
        {
            using var context = CreateTestContext();

            var revokedSession = CreateSession(context.User, "revoked-hash");
            revokedSession.RevokedUtc = DateTime.UtcNow;

            await context.AddSessionsAsync(revokedSession);

            var result = await context.Repository.GetUnrevokedByUserIdAsync(context.User.Id, Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeEmpty();
        }
    }

    private static AuthSessionEntity CreateSession(UserEntity user, string refreshTokenHash)
    {
        return new AuthSessionEntity
        {
            RowId = Guid.NewGuid(),
            User = user,
            RefreshTokenHash = refreshTokenHash,
            CreatedUtc = DateTime.UtcNow,
            ExpiresUtc = DateTime.UtcNow.AddDays(7)
        };
    }

    private static TestContext CreateTestContext()
    {
        var dbOptions = new DbContextOptionsBuilder<PotDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var site = EntityFactory.CreateSite();
        var user = EntityFactory.CreateUser(site, "auth-user");
        var otherUser = EntityFactory.CreateUser(site, "other-auth-user");

        var currentUserContext = Substitute.For<ICurrentUserContext>();
        currentUserContext.UserRowId.Returns(user.RowId);

        var dbContext = new PotDbContext(dbOptions, currentUserContext);

        dbContext.Add(site);
        dbContext.Add(user);
        dbContext.Add(otherUser);
        dbContext.SaveChanges();

        var repository = new AuthSessionRepository(dbContext);

        return new TestContext(dbContext, repository, site, user, otherUser);
    }
}
