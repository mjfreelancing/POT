using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Pot.Data.Entities;
using Pot.Data.Extensions;
using Pot.Shared;
using Pot.TestUtils;
using Shouldly;

namespace Pot.Data.Tests.Extensions;

public class DbContextExtensionsFixture : PotFixtureBase
{
    private sealed class TestContext : IDisposable
    {
        public PotDbContext DbContext { get; }
        public AccountEntity Account { get; }

        public TestContext(PotDbContext dbContext, AccountEntity account)
        {
            DbContext = dbContext;
            Account = account;
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }

    public class WithAutoTracking : DbContextExtensionsFixture
    {
        [Fact]
        public void Should_Not_Track_Entities_Read_Outside_A_Scope()
        {
            using var context = CreateTestContext();

            var account = context.DbContext.Accounts.Single(item => item.RowId == context.Account.RowId);

            context.DbContext.Entry(account).State.ShouldBe(EntityState.Detached);
        }

        [Fact]
        public void Should_Track_Entities_Read_Inside_A_Scope()
        {
            using var context = CreateTestContext();

            using (context.DbContext.WithAutoTracking())
            {
                var account = context.DbContext.Accounts.Single(item => item.RowId == context.Account.RowId);

                context.DbContext.Entry(account).State.ShouldBe(EntityState.Unchanged);
            }
        }

        [Fact]
        public void Should_Enable_And_Restore_Tracking_Around_A_Single_Scope()
        {
            using var context = CreateTestContext();

            context.DbContext.ChangeTracker.QueryTrackingBehavior.ShouldBe(QueryTrackingBehavior.NoTrackingWithIdentityResolution);

            using (context.DbContext.WithAutoTracking())
            {
                context.DbContext.ChangeTracker.QueryTrackingBehavior.ShouldBe(QueryTrackingBehavior.TrackAll);
            }

            context.DbContext.ChangeTracker.QueryTrackingBehavior.ShouldBe(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
        }

        [Fact]
        public void Should_Keep_Tracking_Enabled_While_Nested_Scopes_Remain_Open()
        {
            using var context = CreateTestContext();

            using (context.DbContext.WithAutoTracking())
            {
                using (context.DbContext.WithAutoTracking())
                {
                    context.DbContext.ChangeTracker.QueryTrackingBehavior.ShouldBe(QueryTrackingBehavior.TrackAll);
                }

                // The inner scope must not disable tracking for the outer scope.
                context.DbContext.ChangeTracker.QueryTrackingBehavior.ShouldBe(QueryTrackingBehavior.TrackAll);
            }

            context.DbContext.ChangeTracker.QueryTrackingBehavior.ShouldBe(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
        }

        [Fact]
        public void Should_Restore_Tracking_Across_Sequential_Scopes()
        {
            using var context = CreateTestContext();

            using (context.DbContext.WithAutoTracking())
            {
                context.DbContext.ChangeTracker.QueryTrackingBehavior.ShouldBe(QueryTrackingBehavior.TrackAll);
            }

            context.DbContext.ChangeTracker.QueryTrackingBehavior.ShouldBe(QueryTrackingBehavior.NoTrackingWithIdentityResolution);

            using (context.DbContext.WithAutoTracking())
            {
                context.DbContext.ChangeTracker.QueryTrackingBehavior.ShouldBe(QueryTrackingBehavior.TrackAll);
            }

            context.DbContext.ChangeTracker.QueryTrackingBehavior.ShouldBe(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
        }

        [Fact]
        public void Should_Track_Each_DbContext_Independently()
        {
            using var trackedContext = CreateTestContext();
            using var untrackedContext = CreateTestContext();

            using (trackedContext.DbContext.WithAutoTracking())
            {
                trackedContext.DbContext.ChangeTracker.QueryTrackingBehavior.ShouldBe(QueryTrackingBehavior.TrackAll);
                untrackedContext.DbContext.ChangeTracker.QueryTrackingBehavior.ShouldBe(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
            }

            untrackedContext.DbContext.ChangeTracker.QueryTrackingBehavior.ShouldBe(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
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

        // Reads must come from the store so that tracking behavior is observable.
        dbContext.ChangeTracker.Clear();

        return new TestContext(dbContext, account);
    }
}
