using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Pot.Data.Entities;
using Pot.Data.Repositories.Incomes;
using Pot.Shared;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.Data.Tests.Repositories.Incomes;

public class IncomeRepositoryFixture : PotFixtureBase
{
    private sealed class TestContext : IDisposable
    {
        public PotDbContext DbContext { get; }
        public IncomeRepository Repository { get; }
        public SiteEntity Site { get; }

        public TestContext(PotDbContext dbContext, IncomeRepository repository, SiteEntity site)
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

        public Task<Guid[]> GetRequiredRenewalsAsync(Guid[] accountRowIds, DateOnly asOfDate, CancellationToken cancellationToken = default)
        {
            return Repository.GetRequiredRenewalsAsync(accountRowIds, asOfDate, cancellationToken);
        }

        public Task<List<IncomeEntity>> GetAllIncomesAsync(CancellationToken cancellationToken = default)
        {
            return Repository.GetAllIncomesAsync(cancellationToken);
        }

        public Task<List<IncomeEntity>> GetIncomesAsync(Guid[] rowIds, CancellationToken cancellationToken = default)
        {
            return Repository.GetIncomesAsync(rowIds, cancellationToken);
        }

        public Task<IncomeEntity?> GetIncomeOrDefaultAsync(Guid rowId, CancellationToken cancellationToken = default)
        {
            return Repository.GetIncomeOrDefaultAsync(rowId, cancellationToken);
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }

    public class GetAllIncomesAsync : IncomeRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_All_Incomes_With_Account_Navigation_Loaded()
        {
            using var context = CreateTestContext();

            var account = EntityFactory.CreateAccount(context.Site, "Income Account", 1000.0);
            var income1 = EntityFactory.CreateIncome(account, false, "Salary", 100, "2025-01-10", null, Frequency.Weeks, 1);
            var income2 = EntityFactory.CreateIncome(account, false, "Bonus", 200, "2025-01-12", null, Frequency.Months, 1);

            account.Incomes.Add(income1);
            account.Incomes.Add(income2);

            await context.AddAccountsAsync(account);

            var result = await context.GetAllIncomesAsync();

            result.Count.ShouldBe(2);
            result.ShouldContain(income => income.RowId == income1.RowId && income.Account.RowId == account.RowId);
            result.ShouldContain(income => income.RowId == income2.RowId && income.Account.RowId == account.RowId);
        }
    }

    public class GetIncomesAsync : IncomeRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_Only_Requested_Incomes()
        {
            using var context = CreateTestContext();

            var account = EntityFactory.CreateAccount(context.Site, "Income Account", 1000.0);
            var requestedIncome = EntityFactory.CreateIncome(account, false, "Requested", 100, "2025-01-10", null, Frequency.Weeks, 1);
            var otherIncome = EntityFactory.CreateIncome(account, false, "Other", 200, "2025-01-12", null, Frequency.Months, 1);

            account.Incomes.Add(requestedIncome);
            account.Incomes.Add(otherIncome);

            await context.AddAccountsAsync(account);

            var result = await context.GetIncomesAsync([requestedIncome.RowId]);

            result.Count.ShouldBe(1);
            result[0].RowId.ShouldBe(requestedIncome.RowId);
            result[0].Account.RowId.ShouldBe(account.RowId);
        }
    }

    public class GetIncomeOrDefaultAsync : IncomeRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_Income_When_RowId_Exists()
        {
            using var context = CreateTestContext();

            var account = EntityFactory.CreateAccount(context.Site, "Income Account", 1000.0);
            var income = EntityFactory.CreateIncome(account, false, "Salary", 100, "2025-01-10", null, Frequency.Weeks, 1);

            account.Incomes.Add(income);

            await context.AddAccountsAsync(account);

            var result = await context.GetIncomeOrDefaultAsync(income.RowId);

            result.ShouldNotBeNull();
            result.RowId.ShouldBe(income.RowId);
            result.Account.RowId.ShouldBe(account.RowId);
        }

        [Fact]
        public async Task Should_Return_Null_When_RowId_Does_Not_Exist()
        {
            using var context = CreateTestContext();

            var result = await context.GetIncomeOrDefaultAsync(Guid.NewGuid());

            result.ShouldBeNull();
        }
    }

    public class GetRequiredRenewalsAsync : IncomeRepositoryFixture
    {
        [Fact]
        public async Task Should_Exclude_OneTime_And_Ended_On_AsOfDate_And_Include_Active_Recurring_Incomes()
        {
            using var context = CreateTestContext();

            var asOfDate = new DateOnly(2025, 1, 20);

            var account = EntityFactory.CreateAccount(context.Site, "Income Account", 1000.0);
            var otherAccount = EntityFactory.CreateAccount(context.Site, "Other Account", 500.0);

            var eligibleNoEndDate = EntityFactory.CreateIncome(account, false, "Eligible No EndDate", 100, "2025-01-10", null, Frequency.Weeks, 1);
            var eligibleEndsAfter = EntityFactory.CreateIncome(account, false, "Eligible Ends After", 100, "2025-01-10", "2025-01-21", Frequency.Weeks, 1);

            var endedOnAsOfDate = EntityFactory.CreateIncome(account, false, "Ended On AsOfDate", 100, "2025-01-10", "2025-01-20", Frequency.Weeks, 1);
            var oneTimeIncome = EntityFactory.CreateIncome(account, false, "One Time", 100, "2025-01-10", null, Frequency.OneTime, 1);
            var excludedFromCalcs = EntityFactory.CreateIncome(account, true, "Excluded", 100, "2025-01-10", null, Frequency.Weeks, 1);
            var futureDue = EntityFactory.CreateIncome(account, false, "Future Due", 100, "2025-01-21", null, Frequency.Weeks, 1);

            var otherAccountEligible = EntityFactory.CreateIncome(otherAccount, false, "Other Account Eligible", 100, "2025-01-10", null, Frequency.Weeks, 1);

            account.Incomes.Add(eligibleNoEndDate);
            account.Incomes.Add(eligibleEndsAfter);
            account.Incomes.Add(endedOnAsOfDate);
            account.Incomes.Add(oneTimeIncome);
            account.Incomes.Add(excludedFromCalcs);
            account.Incomes.Add(futureDue);

            otherAccount.Incomes.Add(otherAccountEligible);

            await context.AddAccountsAsync(account, otherAccount);

            var result = await context.GetRequiredRenewalsAsync([account.RowId], asOfDate);

            result.Length.ShouldBe(2);
            result.ShouldContain(eligibleNoEndDate.RowId);
            result.ShouldContain(eligibleEndsAfter.RowId);

            result.ShouldNotContain(endedOnAsOfDate.RowId);
            result.ShouldNotContain(oneTimeIncome.RowId);
            result.ShouldNotContain(excludedFromCalcs.RowId);
            result.ShouldNotContain(futureDue.RowId);
            result.ShouldNotContain(otherAccountEligible.RowId);
        }

        [Fact]
        public async Task Should_Include_Income_When_NextDue_Equals_AsOfDate()
        {
            using var context = CreateTestContext();

            var asOfDate = new DateOnly(2025, 1, 20);
            var account = EntityFactory.CreateAccount(context.Site, "Income Account", 1000.0);
            var dueOnAsOfDate = EntityFactory.CreateIncome(account, false, "Due On AsOfDate", 100, "2025-01-20", null, Frequency.Weeks, 1);

            account.Incomes.Add(dueOnAsOfDate);

            await context.AddAccountsAsync(account);

            var result = await context.GetRequiredRenewalsAsync([account.RowId], asOfDate);

            result.Length.ShouldBe(1);
            result.ShouldContain(dueOnAsOfDate.RowId);
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

        var repository = new IncomeRepository(dbContext);

        return new TestContext(dbContext, repository, site);
    }
}