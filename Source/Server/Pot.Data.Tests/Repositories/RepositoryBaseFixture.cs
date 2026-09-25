using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Shared;
using Pot.TestUtils;
using Shouldly;

namespace Pot.Data.Tests.Repositories;

// AccountRepository is used as the concrete subject because it is a PersistableRepository,
// so exercising it also verifies the shared RepositoryBase behavior.
public class RepositoryBaseFixture : PotFixtureBase
{
    private sealed class TestContext : IDisposable
    {
        public PotDbContext DbContext { get; }
        public AccountRepository Repository { get; }
        public SiteEntity Site { get; }
        public UserEntity User { get; }
        public AccountEntity Account { get; }

        public TestContext(PotDbContext dbContext, AccountRepository repository, SiteEntity site, UserEntity user, AccountEntity account)
        {
            DbContext = dbContext;
            Repository = repository;
            Site = site;
            User = user;
            Account = account;
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }

    public class WithTracking : RepositoryBaseFixture
    {
        [Fact]
        public async Task Should_Track_Entities_Read_Inside_The_Scope()
        {
            using var context = CreateTestContext();

            using (context.Repository.WithTracking())
            {
                var account = await context.Repository.GetAccountAsync(context.Account.RowId, Xunit.TestContext.Current.CancellationToken);

                context.DbContext.Entry(account).State.ShouldBe(EntityState.Unchanged);
            }
        }

        [Fact]
        public void Should_Restore_No_Tracking_After_The_Scope_Is_Disposed()
        {
            using var context = CreateTestContext();

            context.DbContext.ChangeTracker.QueryTrackingBehavior.ShouldBe(QueryTrackingBehavior.NoTrackingWithIdentityResolution);

            using (context.Repository.WithTracking())
            {
                context.DbContext.ChangeTracker.QueryTrackingBehavior.ShouldBe(QueryTrackingBehavior.TrackAll);
            }

            context.DbContext.ChangeTracker.QueryTrackingBehavior.ShouldBe(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
        }

        [Fact]
        public void Should_Keep_Tracking_Enabled_Until_All_Nested_Scopes_Are_Disposed()
        {
            using var context = CreateTestContext();

            using (context.Repository.WithTracking())
            {
                using (context.Repository.WithTracking())
                {
                    context.DbContext.ChangeTracker.QueryTrackingBehavior.ShouldBe(QueryTrackingBehavior.TrackAll);
                }

                // The inner scope must not disable tracking for the outer scope.
                context.DbContext.ChangeTracker.QueryTrackingBehavior.ShouldBe(QueryTrackingBehavior.TrackAll);
            }

            context.DbContext.ChangeTracker.QueryTrackingBehavior.ShouldBe(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
        }
    }

    public class Set : RepositoryBaseFixture
    {
        [Fact]
        public async Task Should_Return_A_Queryable_For_The_Entity_Type()
        {
            using var context = CreateTestContext();

            var queryable = context.Repository.Set<AccountEntity>();

            var result = await queryable.ToListAsync(Xunit.TestContext.Current.CancellationToken);

            result.Count.ShouldBe(1);
            result[0].RowId.ShouldBe(context.Account.RowId);
        }
    }

    public class GetEntry : RepositoryBaseFixture
    {
        [Fact]
        public void Should_Return_The_Change_Tracker_Entry_For_The_Entity()
        {
            using var context = CreateTestContext();

            var entry = context.Repository.GetEntry(context.Account);

            entry.Entity.ShouldBe(context.Account);
            entry.State.ShouldBe(EntityState.Detached);
        }
    }

    public class GetByPrimaryKeyAsync : RepositoryBaseFixture
    {
        [Fact]
        public async Task Should_Return_The_Entity_When_The_Primary_Key_Exists()
        {
            using var context = CreateTestContext();

            var result = await context.Repository.GetByPrimaryKeyAsync<AccountEntity, int>(context.Account.Id, Xunit.TestContext.Current.CancellationToken);

            result.ShouldNotBeNull();
            result.RowId.ShouldBe(context.Account.RowId);
        }

        [Fact]
        public async Task Should_Return_Null_When_The_Primary_Key_Does_Not_Exist()
        {
            using var context = CreateTestContext();

            var result = await context.Repository.GetByPrimaryKeyAsync<AccountEntity, int>(int.MaxValue, Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Return_The_Entity_When_The_Primary_Key_Values_Exist()
        {
            using var context = CreateTestContext();

            var result = await context.Repository.GetByPrimaryKeyAsync<AccountEntity>([context.Account.Id], Xunit.TestContext.Current.CancellationToken);

            result.ShouldNotBeNull();
            result.RowId.ShouldBe(context.Account.RowId);
        }
    }

    private static TestContext CreateTestContext()
    {
        var dbOptions = new DbContextOptionsBuilder<PotDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var site = EntityFactory.CreateSite();
        var user = EntityFactory.CreateUser(site);
        var account = EntityFactory.CreateAccount(site, "Test Account", 1000.0);

        var currentUserContext = Substitute.For<ICurrentUserContext>();
        currentUserContext.UserRowId.Returns(user.RowId);

        var dbContext = new PotDbContext(dbOptions, currentUserContext);

        dbContext.Add(site);
        dbContext.Add(user);
        dbContext.Add(account);
        dbContext.SaveChanges();

        // Reads must come from the store so that the base read behaviors are exercised.
        dbContext.ChangeTracker.Clear();

        var repository = new AccountRepository(dbContext);

        return new TestContext(dbContext, repository, site, user, account);
    }
}
