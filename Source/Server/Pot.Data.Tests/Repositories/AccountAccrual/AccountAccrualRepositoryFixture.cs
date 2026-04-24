using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Pot.Data.Entities;
using Pot.Data.Repositories.AccountAccrual;
using Pot.Shared;
using Pot.TestUtils;
using Shouldly;

namespace Pot.Data.Tests.Repositories.AccountAccrual;

public class AccountAccrualRepositoryFixture : PotFixtureBase
{
    private sealed class TestContext : IDisposable
    {
        public PotDbContext DbContext { get; }
        public AccountAccrualRepository Repository { get; }
        public SiteEntity Site { get; }

        public TestContext(PotDbContext dbContext, AccountAccrualRepository repository, SiteEntity site)
        {
            DbContext = dbContext;
            Repository = repository;
            Site = site;
        }

        public Task<int> AddAccountsAsync(params AccountEntity[] accounts)
        {
            foreach (var account in accounts)
            {
                DbContext.Add(account);
            }

            return DbContext.SaveChangesAsync();
        }

        public Task<Guid[]> GetRequiredAccountAccrualsAsync(Guid[] accountRowIds, DateOnly asOfDate, CancellationToken cancellationToken = default)
        {
            return Repository.GetRequiredAccountAccrualsAsync(accountRowIds, asOfDate, cancellationToken);
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }

    public class GetRequiredAccountAccrualsAsync : AccountAccrualRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_Accounts_That_Are_Dirty_Or_Never_Accrued_Or_Stale()
        {
            using var context = CreateTestContext();

            var asOfDate = new DateOnly(2026, 4, 24);

            var dirtyAccount = EntityFactory.CreateAccount(context.Site, "Dirty Account", 1000.0);
            var neverAccruedAccount = EntityFactory.CreateAccount(context.Site, "Never Accrued Account", 1000.0);
            var staleAccount = EntityFactory.CreateAccount(context.Site, "Stale Account", 1000.0);
            var upToDateAccount = EntityFactory.CreateAccount(context.Site, "Up To Date Account", 1000.0);
            var notRequestedAccount = EntityFactory.CreateAccount(context.Site, "Not Requested Account", 1000.0);

            context.DbContext.AccountAccruals.AddRange(
                new AccountAccrualEntity
                {
                    Account = dirtyAccount,
                    AccruedIsDirty = true,
                    LastAccruedDate = asOfDate
                },
                new AccountAccrualEntity
                {
                    Account = neverAccruedAccount,
                    AccruedIsDirty = false,
                    LastAccruedDate = null
                },
                new AccountAccrualEntity
                {
                    Account = staleAccount,
                    AccruedIsDirty = false,
                    LastAccruedDate = asOfDate.AddDays(-1)
                },
                new AccountAccrualEntity
                {
                    Account = upToDateAccount,
                    AccruedIsDirty = false,
                    LastAccruedDate = asOfDate
                },
                new AccountAccrualEntity
                {
                    Account = notRequestedAccount,
                    AccruedIsDirty = true,
                    LastAccruedDate = asOfDate
                });

            await context.AddAccountsAsync(dirtyAccount, neverAccruedAccount, staleAccount, upToDateAccount, notRequestedAccount);

            var result = await context.GetRequiredAccountAccrualsAsync(
            [
                dirtyAccount.RowId,
                neverAccruedAccount.RowId,
                staleAccount.RowId,
                upToDateAccount.RowId
            ],
            asOfDate);

            result.Length.ShouldBe(3);
            result.ShouldContain(dirtyAccount.RowId);
            result.ShouldContain(neverAccruedAccount.RowId);
            result.ShouldContain(staleAccount.RowId);
            result.ShouldNotContain(upToDateAccount.RowId);
            result.ShouldNotContain(notRequestedAccount.RowId);
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

        var repository = new AccountAccrualRepository(dbContext, NullLogger<AccountAccrualRepository>.Instance);

        return new TestContext(dbContext, repository, site);
    }
}
