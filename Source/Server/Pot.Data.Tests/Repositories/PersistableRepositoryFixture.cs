using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Shared;
using Pot.TestUtils;
using Shouldly;

namespace Pot.Data.Tests.Repositories;

// AccountRepository is used as the concrete subject because it is a PersistableRepository,
// so exercising it verifies the shared add/update/delete/save behavior.
public class PersistableRepositoryFixture : PotFixtureBase
{
    private sealed class TestContext : IDisposable
    {
        public PotDbContext DbContext { get; }
        public AccountRepository Repository { get; }
        public SiteEntity Site { get; }
        public UserEntity User { get; }

        public TestContext(PotDbContext dbContext, AccountRepository repository, SiteEntity site, UserEntity user)
        {
            DbContext = dbContext;
            Repository = repository;
            Site = site;
            User = user;
        }

        public Task<int> AddAccountAsync(AccountEntity account)
        {
            DbContext.Add(account);
            return DbContext.SaveChangesAsync();
        }

        public Task<AccountEntity> GetAccountAsync(Guid rowId, CancellationToken cancellationToken)
        {
            return DbContext.Accounts.SingleAsync(account => account.RowId == rowId, cancellationToken);
        }

        public Task<int> CountAccountsAsync(CancellationToken cancellationToken)
        {
            return DbContext.Accounts.CountAsync(cancellationToken);
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }

    public class Add : PersistableRepositoryFixture
    {
        [Fact]
        public void Should_Track_The_Entity_As_Added()
        {
            using var context = CreateTestContext();

            var account = EntityFactory.CreateAccount(context.Site, "New Account", 500.0);

            var entry = context.Repository.Add(account);

            entry.State.ShouldBe(EntityState.Added);
        }

        [Fact]
        public async Task Should_Persist_The_Entity_When_Saved()
        {
            using var context = CreateTestContext();

            var account = EntityFactory.CreateAccount(context.Site, "New Account", 500.0);

            context.Repository.Add(account);
            var result = await context.Repository.SaveAsync(Xunit.TestContext.Current.CancellationToken);

            result.ShouldBe(1);
            (await context.CountAccountsAsync(Xunit.TestContext.Current.CancellationToken)).ShouldBe(1);
        }
    }

    public class AddAndSaveAsync : PersistableRepositoryFixture
    {
        [Fact]
        public async Task Should_Add_And_Persist_The_Entity()
        {
            using var context = CreateTestContext();

            var account = EntityFactory.CreateAccount(context.Site, "New Account", 500.0);

            var result = await context.Repository.AddAndSaveAsync(account, Xunit.TestContext.Current.CancellationToken);

            result.ShouldBe(1);

            var savedAccount = await context.GetAccountAsync(account.RowId, Xunit.TestContext.Current.CancellationToken);
            savedAccount.Description.ShouldBe("New Account");
            savedAccount.Balance.ShouldBe(500.0);
        }
    }

    public class Update : PersistableRepositoryFixture
    {
        [Fact]
        public async Task Should_Track_The_Entity_As_Modified()
        {
            using var context = CreateTestContext();

            var account = EntityFactory.CreateAccount(context.Site, "Test Account", 1000.0);
            await context.AddAccountAsync(account);

            account.Balance = 2000.0;

            var entry = context.Repository.Update(account);

            entry.State.ShouldBe(EntityState.Modified);
        }

        [Fact]
        public async Task Should_Persist_The_Changes_When_Saved()
        {
            using var context = CreateTestContext();

            var account = EntityFactory.CreateAccount(context.Site, "Test Account", 1000.0);
            await context.AddAccountAsync(account);

            account.Balance = 2000.0;

            context.Repository.Update(account);
            await context.Repository.SaveAsync(Xunit.TestContext.Current.CancellationToken);

            var savedAccount = await context.GetAccountAsync(account.RowId, Xunit.TestContext.Current.CancellationToken);
            savedAccount.Balance.ShouldBe(2000.0);
        }
    }

    public class UpdateAndSaveAsync : PersistableRepositoryFixture
    {
        [Fact]
        public async Task Should_Update_And_Persist_The_Entity()
        {
            using var context = CreateTestContext();

            var account = EntityFactory.CreateAccount(context.Site, "Test Account", 1000.0);
            await context.AddAccountAsync(account);

            account.Description = "Updated Account";

            var result = await context.Repository.UpdateAndSaveAsync(account, Xunit.TestContext.Current.CancellationToken);

            result.ShouldBe(1);

            var savedAccount = await context.GetAccountAsync(account.RowId, Xunit.TestContext.Current.CancellationToken);
            savedAccount.Description.ShouldBe("Updated Account");
        }
    }

    public class Delete : PersistableRepositoryFixture
    {
        [Fact]
        public async Task Should_Track_The_Entity_As_Deleted()
        {
            using var context = CreateTestContext();

            var account = EntityFactory.CreateAccount(context.Site, "Test Account", 1000.0);
            await context.AddAccountAsync(account);

            var entry = context.Repository.Delete(account);

            entry.State.ShouldBe(EntityState.Deleted);
        }

        [Fact]
        public async Task Should_Remove_The_Entity_When_Saved()
        {
            using var context = CreateTestContext();

            var account = EntityFactory.CreateAccount(context.Site, "Test Account", 1000.0);
            await context.AddAccountAsync(account);

            context.Repository.Delete(account);
            var result = await context.Repository.SaveAsync(Xunit.TestContext.Current.CancellationToken);

            result.ShouldBe(1);
            (await context.CountAccountsAsync(Xunit.TestContext.Current.CancellationToken)).ShouldBe(0);
        }
    }

    public class Save : PersistableRepositoryFixture
    {
        [Fact]
        public void Should_Persist_The_Changes()
        {
            using var context = CreateTestContext();

            var account = EntityFactory.CreateAccount(context.Site, "New Account", 500.0);
            context.Repository.Add(account);

            var result = context.Repository.Save();

            result.ShouldBe(1);
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

        var repository = new AccountRepository(dbContext);

        return new TestContext(dbContext, repository, site, user);
    }
}
