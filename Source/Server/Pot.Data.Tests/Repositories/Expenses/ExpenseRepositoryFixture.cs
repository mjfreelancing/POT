using AllOverIt.Pagination;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Pot.Data.Entities;
using Pot.Data.Repositories.Expenses;
using Pot.Shared;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.Data.Tests.Repositories.Expenses;

public class ExpenseRepositoryFixture : PotFixtureBase
{
    private sealed class TestContext : IDisposable
    {
        public PotDbContext DbContext { get; }
        public ExpenseRepository Repository { get; }
        public SiteEntity Site { get; }

        public TestContext(PotDbContext dbContext, ExpenseRepository repository, SiteEntity site)
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

        public Task<List<ExpenseEntity>> GetAllExpensesAsync(CancellationToken cancellationToken = default)
        {
            return Repository.GetAllExpensesAsync(cancellationToken);
        }

        public Task<PageResult<ExpenseEntity>> GetAllExpensesPagedAsync(Paging paging, CancellationToken cancellationToken = default)
        {
            return Repository.GetAllExpensesPagedAsync(paging, cancellationToken);
        }

        public Task<ExpenseEntity?> GetExpenseOrDefaultAsync(Guid rowId, CancellationToken cancellationToken = default)
        {
            return Repository.GetExpenseOrDefaultAsync(rowId, cancellationToken);
        }

        public Task<List<ExpenseEntity>> GetExpensesAsync(Guid[] rowIds, CancellationToken cancellationToken = default)
        {
            return Repository.GetExpensesAsync(rowIds, cancellationToken);
        }

        public Task<List<ExpenseEntity>> GetExpensesForAccountAsync(Guid accountRowId, CancellationToken cancellationToken = default)
        {
            return Repository.GetExpensesForAccountAsync(accountRowId, cancellationToken);
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

    public class GetAllExpensesAsync : ExpenseRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_All_Expenses_With_Account_Navigation_Loaded()
        {
            using var context = CreateTestContext();

            var account = EntityFactory.CreateAccount(context.Site, "Expense Account", 1000.0);
            var expense1 = EntityFactory.CreateExpense(account, false, "Rent", 100, "2025-01-01", "2025-01-10", null, Frequency.Months, 1);
            var expense2 = EntityFactory.CreateExpense(account, false, "Utilities", 50, "2025-01-01", "2025-01-15", null, Frequency.Weeks, 1);

            account.Expenses.Add(expense1);
            account.Expenses.Add(expense2);

            await context.AddAccountsAsync(account);

            var result = await context.GetAllExpensesAsync();

            result.Count.ShouldBe(2);
            result.ShouldContain(expense => expense.RowId == expense1.RowId && expense.Account.RowId == account.RowId);
            result.ShouldContain(expense => expense.RowId == expense2.RowId && expense.Account.RowId == account.RowId);
        }
    }

    public class GetAllExpensesPagedAsync : ExpenseRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_First_Page_Using_Repository_Paging()
        {
            using var context = CreateTestContext();

            var account = EntityFactory.CreateAccount(context.Site, "Expense Account", 1000.0);
            var expense1 = EntityFactory.CreateExpense(account, false, "A Expense", 100, "2025-01-01", "2025-01-10", null, Frequency.Months, 1);
            var expense2 = EntityFactory.CreateExpense(account, false, "B Expense", 100, "2025-01-01", "2025-01-11", null, Frequency.Months, 1);
            var expense3 = EntityFactory.CreateExpense(account, false, "C Expense", 100, "2025-01-01", "2025-01-12", null, Frequency.Months, 1);

            account.Expenses.Add(expense1);
            account.Expenses.Add(expense2);
            account.Expenses.Add(expense3);

            await context.AddAccountsAsync(account);

            var paging = new Paging
            {
                Limit = 2,
                Continuation = null
            };

            var result = await context.GetAllExpensesPagedAsync(paging);

            result.ShouldNotBeNull();

            result.Results[0].RowId.ShouldBe(expense1.RowId);
            result.Results[1].RowId.ShouldBe(expense2.RowId);

            result.CurrentToken.ShouldBeNull();
            result.PreviousToken.ShouldBeNull();
            result.NextToken.ShouldNotBeNull();
            result.TotalCount.ShouldBe(3);
        }

        [Fact]
        public async Task Should_Order_By_NextDue_Then_Description_Then_Id()
        {
            using var context = CreateTestContext();

            var account = EntityFactory.CreateAccount(context.Site, "Expense Account", 1000.0);

            var sameDueEarlierDescription = EntityFactory.CreateExpense(account, false, "A Expense", 100, "2025-01-01", "2025-01-10", null, Frequency.Months, 1);
            var sameDueSameDescriptionFirst = EntityFactory.CreateExpense(account, false, "B Expense", 100, "2025-01-01", "2025-01-10", null, Frequency.Months, 1);
            var sameDueSameDescriptionSecond = EntityFactory.CreateExpense(account, false, "B Expense", 100, "2025-01-01", "2025-01-10", null, Frequency.Months, 1);

            account.Expenses.Add(sameDueSameDescriptionSecond);
            account.Expenses.Add(sameDueEarlierDescription);
            account.Expenses.Add(sameDueSameDescriptionFirst);

            await context.AddAccountsAsync(account);

            var paging = new Paging
            {
                Limit = 3,
                Continuation = null
            };

            var result = await context.GetAllExpensesPagedAsync(paging);

            result.Results.Length.ShouldBe(3);
            result.Results[0].RowId.ShouldBe(sameDueEarlierDescription.RowId);
            result.Results[1].RowId.ShouldBe(sameDueSameDescriptionSecond.RowId);
            result.Results[2].RowId.ShouldBe(sameDueSameDescriptionFirst.RowId);
        }
    }

    public class GetExpenseOrDefaultAsync : ExpenseRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_Expense_When_RowId_Exists()
        {
            using var context = CreateTestContext();

            var account = EntityFactory.CreateAccount(context.Site, "Expense Account", 1000.0);
            var expense = EntityFactory.CreateExpense(account, false, "Rent", 100, "2025-01-01", "2025-01-10", null, Frequency.Weeks, 1);

            account.Expenses.Add(expense);

            await context.AddAccountsAsync(account);

            var result = await context.GetExpenseOrDefaultAsync(expense.RowId);

            result.ShouldNotBeNull();
            result.RowId.ShouldBe(expense.RowId);
            result.Account.RowId.ShouldBe(account.RowId);
        }

        [Fact]
        public async Task Should_Return_Null_When_RowId_Does_Not_Exist()
        {
            using var context = CreateTestContext();

            var result = await context.GetExpenseOrDefaultAsync(Guid.NewGuid());

            result.ShouldBeNull();
        }
    }

    public class GetExpensesAsync : ExpenseRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_Only_Requested_Expenses()
        {
            using var context = CreateTestContext();

            var account = EntityFactory.CreateAccount(context.Site, "Expense Account", 1000.0);
            var requestedExpense = EntityFactory.CreateExpense(account, false, "Requested", 100, "2025-01-01", "2025-01-10", null, Frequency.Weeks, 1);
            var otherExpense = EntityFactory.CreateExpense(account, false, "Other", 100, "2025-01-01", "2025-01-17", null, Frequency.Weeks, 1);

            account.Expenses.Add(requestedExpense);
            account.Expenses.Add(otherExpense);

            await context.AddAccountsAsync(account);

            var result = await context.GetExpensesAsync([requestedExpense.RowId]);

            result.Count.ShouldBe(1);
            result[0].RowId.ShouldBe(requestedExpense.RowId);
            result[0].Account.RowId.ShouldBe(account.RowId);
        }
    }

    public class GetExpensesForAccountAsync : ExpenseRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_All_Expenses_For_Account_Including_Excluded_From_Calcs()
        {
            using var context = CreateTestContext();

            var account = EntityFactory.CreateAccount(context.Site, "Expense Account", 1000.0);
            var otherAccount = EntityFactory.CreateAccount(context.Site, "Other Account", 500.0);

            var includedExpense = EntityFactory.CreateExpense(account, false, "Included", 100, "2025-01-01", "2025-01-10", null, Frequency.Weeks, 1);
            var excludedExpense = EntityFactory.CreateExpense(account, true, "Excluded", 100, "2025-01-01", "2025-01-10", null, Frequency.Weeks, 1);
            var otherAccountExpense = EntityFactory.CreateExpense(otherAccount, false, "Other Account Expense", 100, "2025-01-01", "2025-01-10", null, Frequency.Weeks, 1);

            account.Expenses.Add(includedExpense);
            account.Expenses.Add(excludedExpense);
            otherAccount.Expenses.Add(otherAccountExpense);

            await context.AddAccountsAsync(account, otherAccount);

            var result = await context.GetExpensesForAccountAsync(account.RowId);

            result.Count.ShouldBe(2);
            result.ShouldContain(expense => expense.RowId == includedExpense.RowId);
            result.ShouldContain(expense => expense.RowId == excludedExpense.RowId);
            result.ShouldNotContain(expense => expense.RowId == otherAccountExpense.RowId);
        }
    }

    public class GetRequiredRenewalsAsync : ExpenseRepositoryFixture
    {
        [Fact]
        public async Task Should_Exclude_OneTime_And_Ended_On_AsOfDate_And_Include_Active_Recurring_Expenses()
        {
            using var context = CreateTestContext();

            var asOfDate = new DateOnly(2025, 1, 20);

            var account = EntityFactory.CreateAccount(context.Site, "Expense Account", 1000.0);
            var otherAccount = EntityFactory.CreateAccount(context.Site, "Other Account", 500.0);

            var eligibleNoEndDate = EntityFactory.CreateExpense(account, false, "Eligible No EndDate", 100, "2025-01-01", "2025-01-10", null, Frequency.Weeks, 1);
            var eligibleEndsAfter = EntityFactory.CreateExpense(account, false, "Eligible Ends After", 100, "2025-01-01", "2025-01-10", "2025-01-21", Frequency.Weeks, 1);

            var endedOnAsOfDate = EntityFactory.CreateExpense(account, false, "Ended On AsOfDate", 100, "2025-01-01", "2025-01-10", "2025-01-20", Frequency.Weeks, 1);
            var oneTimeExpense = EntityFactory.CreateExpense(account, false, "One Time", 100, "2025-01-01", "2025-01-10", null, Frequency.OneTime, 1);
            var excludedFromCalcs = EntityFactory.CreateExpense(account, true, "Excluded", 100, "2025-01-01", "2025-01-10", null, Frequency.Weeks, 1);
            var futureDue = EntityFactory.CreateExpense(account, false, "Future Due", 100, "2025-01-01", "2025-01-21", null, Frequency.Weeks, 1);

            var otherAccountEligible = EntityFactory.CreateExpense(otherAccount, false, "Other Account Eligible", 100, "2025-01-01", "2025-01-10", null, Frequency.Weeks, 1);

            account.Expenses.Add(eligibleNoEndDate);
            account.Expenses.Add(eligibleEndsAfter);
            account.Expenses.Add(endedOnAsOfDate);
            account.Expenses.Add(oneTimeExpense);
            account.Expenses.Add(excludedFromCalcs);
            account.Expenses.Add(futureDue);

            otherAccount.Expenses.Add(otherAccountEligible);

            await context.AddAccountsAsync(account, otherAccount);

            var result = await context.GetRequiredRenewalsAsync([account.RowId], asOfDate);

            result.Length.ShouldBe(2);
            result.ShouldContain(eligibleNoEndDate.RowId);
            result.ShouldContain(eligibleEndsAfter.RowId);

            result.ShouldNotContain(endedOnAsOfDate.RowId);
            result.ShouldNotContain(oneTimeExpense.RowId);
            result.ShouldNotContain(excludedFromCalcs.RowId);
            result.ShouldNotContain(futureDue.RowId);
            result.ShouldNotContain(otherAccountEligible.RowId);
        }

        [Fact]
        public async Task Should_Include_Expense_When_NextDue_Equals_AsOfDate()
        {
            using var context = CreateTestContext();

            var asOfDate = new DateOnly(2025, 1, 20);

            var account = EntityFactory.CreateAccount(context.Site, "Expense Account", 1000.0);
            var dueOnAsOfDate = EntityFactory.CreateExpense(account, false, "Due On AsOfDate", 100, "2025-01-01", "2025-01-20", null, Frequency.Weeks, 1);

            account.Expenses.Add(dueOnAsOfDate);

            await context.AddAccountsAsync(account);

            var result = await context.GetRequiredRenewalsAsync([account.RowId], asOfDate);

            result.Length.ShouldBe(1);
            result.ShouldContain(dueOnAsOfDate.RowId);
        }
    }

    public class GetRequiredAccountAccrualsAsync : ExpenseRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_Distinct_Account_RowIds_That_Require_Accrual_Update()
        {
            using var context = CreateTestContext();

            var asOfDate = new DateOnly(2025, 1, 20);

            var account = EntityFactory.CreateAccount(context.Site, "Expense Account", 1000.0);
            var otherAccount = EntityFactory.CreateAccount(context.Site, "Other Account", 500.0);

            var dirtyExpense = EntityFactory.CreateExpense(account, false, "Dirty", 100, "2025-01-01", "2025-01-10", null, Frequency.Weeks, 1);
            dirtyExpense.AccruedIsDirty = true;
            dirtyExpense.LastAccruedUpdate = asOfDate;

            var staleExpense = EntityFactory.CreateExpense(account, true, "Stale", 100, "2025-01-01", "2025-01-10", null, Frequency.Weeks, 1);
            staleExpense.AccruedIsDirty = false;
            staleExpense.LastAccruedUpdate = asOfDate.AddDays(-1);

            var upToDateExpense = EntityFactory.CreateExpense(account, false, "UpToDate", 100, "2025-01-01", "2025-01-10", null, Frequency.Weeks, 1);
            upToDateExpense.AccruedIsDirty = false;
            upToDateExpense.LastAccruedUpdate = asOfDate;

            var noAccrualNeededOtherAccount = EntityFactory.CreateExpense(otherAccount, false, "Other UpToDate", 100, "2025-01-01", "2025-01-10", null, Frequency.Weeks, 1);
            noAccrualNeededOtherAccount.AccruedIsDirty = false;
            noAccrualNeededOtherAccount.LastAccruedUpdate = asOfDate;

            account.Expenses.Add(dirtyExpense);
            account.Expenses.Add(staleExpense);
            account.Expenses.Add(upToDateExpense);
            otherAccount.Expenses.Add(noAccrualNeededOtherAccount);

            await context.AddAccountsAsync(account, otherAccount);

            var result = await context.GetRequiredAccountAccrualsAsync([account.RowId, otherAccount.RowId], asOfDate);

            result.Length.ShouldBe(1);
            result[0].ShouldBe(account.RowId);
        }

        [Fact]
        public async Task Should_Include_Account_When_LastAccruedUpdate_Is_Null()
        {
            using var context = CreateTestContext();

            var asOfDate = new DateOnly(2025, 1, 20);
            var account = EntityFactory.CreateAccount(context.Site, "Expense Account", 1000.0);

            var missingAccrualExpense = EntityFactory.CreateExpense(account, false, "Missing Accrual", 100, "2025-01-01", "2025-01-10", null, Frequency.Weeks, 1);
            missingAccrualExpense.AccruedIsDirty = false;
            missingAccrualExpense.LastAccruedUpdate = null;

            account.Expenses.Add(missingAccrualExpense);

            await context.AddAccountsAsync(account);

            var result = await context.GetRequiredAccountAccrualsAsync([account.RowId], asOfDate);

            result.Length.ShouldBe(1);
            result[0].ShouldBe(account.RowId);
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

        var queryPaginatorFactory = Substitute.For<IQueryPaginatorFactory>();

        queryPaginatorFactory
            .CreatePaginator(Arg.Any<IQueryable<ExpenseEntity>>(), Arg.Any<QueryPaginatorConfiguration>())
            .Returns(callInfo => QueryPaginator<ExpenseEntity>.Create(
                callInfo.ArgAt<IQueryable<ExpenseEntity>>(0),
                callInfo.ArgAt<QueryPaginatorConfiguration>(1)));

        var repository = new ExpenseRepository(dbContext, queryPaginatorFactory);

        return new TestContext(dbContext, repository, site);
    }
}