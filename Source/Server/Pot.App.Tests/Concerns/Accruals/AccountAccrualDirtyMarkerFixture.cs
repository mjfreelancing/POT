using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using Pot.App.Concerns.Accruals;
using Pot.Data;
using Pot.Data.Entities;
using Pot.Data.Repositories.AccountAccrual;
using Pot.Shared;
using Pot.TestUtils;
using Pot.TestUtils.Logging;
using Shouldly;

namespace Pot.App.Tests.Concerns.Accruals;

public class AccountAccrualDirtyMarkerFixture : PotFixtureBase
{
    private sealed class TestContext : IDisposable
    {
        public AccountAccrualDirtyMarker Marker { get; }
        public PotDbContext DbContext { get; }
        public SiteEntity Site { get; }
        public FakeLogCollector LogCollector { get; }

        public TestContext(AccountAccrualDirtyMarker marker, PotDbContext dbContext, SiteEntity site, FakeLogCollector logCollector)
        {
            Marker = marker;
            DbContext = dbContext;
            Site = site;
            LogCollector = logCollector;
        }

        public AccountEntity AddAccount(string description)
        {
            var account = EntityFactory.CreateAccount(Site, description, balance: 1000.0d);

            DbContext.Accounts.Add(account);
            DbContext.SaveChanges();

            return account;
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }

    public class Constructor : AccountAccrualDirtyMarkerFixture
    {
        [Fact]
        public void Should_Throw_When_AccountAccrualRepository_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var logger = Substitute.For<ILogger<AccountAccrualDirtyMarker>>();
                _ = new AccountAccrualDirtyMarker(null!, logger);
            });

            exception.ParamName.ShouldBe("accountAccrualRepository");
        }

        [Fact]
        public void Should_Throw_When_Logger_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var repository = Substitute.For<IPersistableAccountAccrualRepository>();
                _ = new AccountAccrualDirtyMarker(repository, null!);
            });

            exception.ParamName.ShouldBe("logger");
        }
    }

    public class MarkDirtyForCreateAsync : AccountAccrualDirtyMarkerFixture
    {
        [Fact]
        public async Task Should_Throw_When_Account_Is_Null()
        {
            using var context = CreateTestContext();

            var exception = await Should.ThrowAsync<ArgumentNullException>(async () =>
            {
                await context.Marker.MarkDirtyForCreateAsync(null!, CancellationToken.None);
            });

            exception.ParamName.ShouldBe("account");
        }

        [Fact]
        public async Task Should_Add_AccountAccrual_When_Missing()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Create Branch Account");

            context.DbContext.AccountAccruals.Count().ShouldBe(0);

            await context.Marker.MarkDirtyForCreateAsync(account, CancellationToken.None);

            var accountAccrualEntry = context.DbContext.ChangeTracker
                .Entries<AccountAccrualEntity>()
                .Single();

            accountAccrualEntry.State.ShouldBe(EntityState.Added);
            accountAccrualEntry.Entity.AccountId.ShouldBe(account.Id);
            accountAccrualEntry.Entity.AccruedIsDirty.ShouldBeTrue();
            context.DbContext.ChangeTracker.HasChanges().ShouldBeTrue();
        }

        [Fact]
        public async Task Should_Set_AccruedIsDirty_True_When_Existing_Row_Is_Clean()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Clean Branch Account");

            context.DbContext.AccountAccruals.Add(new AccountAccrualEntity
            {
                AccountId = account.Id,
                Account = account,
                AccruedIsDirty = false,
                LastAccruedDate = new DateOnly(2026, 4, 1)
            });

            await context.DbContext.SaveChangesAsync();
            context.DbContext.ChangeTracker.Clear();

            await context.Marker.MarkDirtyForCreateAsync(account, CancellationToken.None);

            var accountAccrualEntry = context.DbContext.ChangeTracker
                .Entries<AccountAccrualEntity>()
                .Single();

            accountAccrualEntry.State.ShouldBe(EntityState.Modified);
            accountAccrualEntry.Entity.AccruedIsDirty.ShouldBeTrue();
            context.DbContext.ChangeTracker.HasChanges().ShouldBeTrue();
        }

        [Fact]
        public async Task Should_Not_Modify_When_Existing_Row_Already_Dirty()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Dirty Branch Account");

            context.DbContext.AccountAccruals.Add(new AccountAccrualEntity
            {
                AccountId = account.Id,
                Account = account,
                AccruedIsDirty = true,
                LastAccruedDate = new DateOnly(2026, 4, 2)
            });

            await context.DbContext.SaveChangesAsync();
            context.DbContext.ChangeTracker.Clear();

            await context.Marker.MarkDirtyForCreateAsync(account, CancellationToken.None);

            var accountAccrual = await context.DbContext.AccountAccruals.SingleAsync();

            accountAccrual.AccruedIsDirty.ShouldBeTrue();
            context.DbContext.ChangeTracker.HasChanges().ShouldBeFalse();
        }

        [Fact]
        public async Task Should_LogCall_When_Marking_Dirty_For_Create()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Logging Branch Account");

            await context.Marker.MarkDirtyForCreateAsync(account, CancellationToken.None);

            context.LogCollector.ShouldContainLogCall(
                category: typeof(AccountAccrualDirtyMarker).FullName!,
                callerName: nameof(AccountAccrualDirtyMarker.MarkDirtyForCreateAsync),
                callerType: typeof(AccountAccrualDirtyMarker));
        }
    }

    private static TestContext CreateTestContext()
    {
        var dbOptions = new DbContextOptionsBuilder<PotDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var site = EntityFactory.CreateSite();
        var user = EntityFactory.CreateUser(site);

        var currentUserContext = Substitute.For<ICurrentUserContext>();
        currentUserContext.UserRowId.Returns(user.RowId);

        var dbContext = new PotDbContext(dbOptions, currentUserContext);

        dbContext.Add(site);
        dbContext.Add(user);
        dbContext.SaveChanges();

        var repositoryLogger = new FakeLogger<AccountAccrualRepository>();
        var markerLogCollector = new FakeLogCollector();
        var markerLogger = new FakeLogger<AccountAccrualDirtyMarker>(markerLogCollector);

        var repository = new AccountAccrualRepository(dbContext, repositoryLogger);
        var marker = new AccountAccrualDirtyMarker(repository, markerLogger);

        return new TestContext(marker, dbContext, site, markerLogCollector);
    }
}
