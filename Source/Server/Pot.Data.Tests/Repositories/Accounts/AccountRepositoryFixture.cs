using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Shared;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.Data.Tests.Repositories.Accounts;

public class AccountRepositoryFixture : PotFixtureBase
{
    private sealed class TestContext : IDisposable
    {
        public PotDbContext DbContext { get; }
        public AccountRepository Repository { get; }
        public SiteEntity Site { get; }
        public SiteEntity OtherSite { get; }

        public TestContext(PotDbContext dbContext, AccountRepository repository, SiteEntity site, SiteEntity otherSite)
        {
            DbContext = dbContext;
            Repository = repository;
            Site = site;
            OtherSite = otherSite;
        }

        public Task<int> AddAccountsAsync(params AccountEntity[] accounts)
        {
            foreach (var account in accounts)
            {
                DbContext.Add(account);
            }

            return DbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }

    public class AccountExistsAsync : AccountRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_True_When_Account_Exists()
        {
            using var context = CreateTestContext();

            var account = EntityFactory.CreateAccount(context.Site, "Test Account", 1000.0);
            await context.AddAccountsAsync(account);

            var result = await context.Repository.AccountExistsAsync(account.RowId, Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_Return_False_When_Account_Does_Not_Exist()
        {
            using var context = CreateTestContext();

            var result = await context.Repository.AccountExistsAsync(Guid.NewGuid(), Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeFalse();
        }

        [Fact]
        public async Task Should_Return_False_When_Account_Belongs_To_Another_Site()
        {
            using var context = CreateTestContext();

            var otherSiteAccount = EntityFactory.CreateAccount(context.OtherSite, "Other Site Account", 1000.0);
            await context.AddAccountsAsync(otherSiteAccount);

            var result = await context.Repository.AccountExistsAsync(otherSiteAccount.RowId, Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeFalse();
        }
    }

    public class HasExpensesAsync : AccountRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_True_When_Account_Has_Expenses()
        {
            using var context = CreateTestContext();

            var account = EntityFactory.CreateAccount(context.Site, "Test Account", 1000.0);
            var expense = EntityFactory.CreateExpense(account, false, "Rent", 100.0, "2025-01-01", "2025-01-10", null, Frequency.Weeks, 1);
            account.Expenses.Add(expense);

            await context.AddAccountsAsync(account);

            var result = await context.Repository.HasExpensesAsync(account.RowId, Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_Return_False_When_Account_Has_No_Expenses()
        {
            using var context = CreateTestContext();

            var account = EntityFactory.CreateAccount(context.Site, "Test Account", 1000.0);
            await context.AddAccountsAsync(account);

            var result = await context.Repository.HasExpensesAsync(account.RowId, Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeFalse();
        }

        [Fact]
        public async Task Should_Return_False_When_Account_Does_Not_Exist()
        {
            using var context = CreateTestContext();

            var result = await context.Repository.HasExpensesAsync(Guid.NewGuid(), Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeFalse();
        }
    }

    public class HasIncomesAsync : AccountRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_True_When_Account_Has_Incomes()
        {
            using var context = CreateTestContext();

            var account = EntityFactory.CreateAccount(context.Site, "Test Account", 1000.0);
            var income = EntityFactory.CreateIncome(account, false, "Salary", 100.0, "2025-01-10", null, Frequency.Weeks, 1);
            account.Incomes.Add(income);

            await context.AddAccountsAsync(account);

            var result = await context.Repository.HasIncomesAsync(account.RowId, Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_Return_False_When_Account_Has_No_Incomes()
        {
            using var context = CreateTestContext();

            var account = EntityFactory.CreateAccount(context.Site, "Test Account", 1000.0);
            await context.AddAccountsAsync(account);

            var result = await context.Repository.HasIncomesAsync(account.RowId, Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeFalse();
        }

        [Fact]
        public async Task Should_Return_False_When_Account_Does_Not_Exist()
        {
            using var context = CreateTestContext();

            var result = await context.Repository.HasIncomesAsync(Guid.NewGuid(), Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeFalse();
        }
    }

    public class GetAccountAsync : AccountRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_The_Account_When_It_Exists()
        {
            using var context = CreateTestContext();

            var account = EntityFactory.CreateAccount(context.Site, "Test Account", 1000.0);
            await context.AddAccountsAsync(account);

            var result = await context.Repository.GetAccountAsync(account.RowId, Xunit.TestContext.Current.CancellationToken);

            result.RowId.ShouldBe(account.RowId);
            result.Description.ShouldBe("Test Account");
            result.Balance.ShouldBe(1000.0);
        }

        [Fact]
        public async Task Should_Throw_When_The_Account_Does_Not_Exist()
        {
            using var context = CreateTestContext();

            await Should.ThrowAsync<InvalidOperationException>(
                () => context.Repository.GetAccountAsync(Guid.NewGuid(), Xunit.TestContext.Current.CancellationToken));
        }
    }

    public class GetAccountOrDefaultAsync : AccountRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_The_Account_When_It_Exists()
        {
            using var context = CreateTestContext();

            var account = EntityFactory.CreateAccount(context.Site, "Test Account", 1000.0);
            await context.AddAccountsAsync(account);

            var result = await context.Repository.GetAccountOrDefaultAsync(account.RowId, Xunit.TestContext.Current.CancellationToken);

            result.ShouldNotBeNull();
            result.RowId.ShouldBe(account.RowId);
        }

        [Fact]
        public async Task Should_Return_Null_When_The_Account_Does_Not_Exist()
        {
            using var context = CreateTestContext();

            var result = await context.Repository.GetAccountOrDefaultAsync(Guid.NewGuid(), Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Return_Null_When_The_Account_Belongs_To_Another_Site()
        {
            using var context = CreateTestContext();

            var otherSiteAccount = EntityFactory.CreateAccount(context.OtherSite, "Other Site Account", 1000.0);
            await context.AddAccountsAsync(otherSiteAccount);

            var result = await context.Repository.GetAccountOrDefaultAsync(otherSiteAccount.RowId, Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeNull();
        }
    }

    public class GetAccountWithLinkedCountsOrDefaultAsync : AccountRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_The_Account_With_Linked_Income_And_Expense_Counts()
        {
            using var context = CreateTestContext();

            var account = EntityFactory.CreateAccount(context.Site, "Test Account", 1000.0);
            var income1 = EntityFactory.CreateIncome(account, false, "Salary", 100.0, "2025-01-10", null, Frequency.Weeks, 1);
            var income2 = EntityFactory.CreateIncome(account, false, "Bonus", 100.0, "2025-01-11", null, Frequency.Weeks, 1);
            var expense = EntityFactory.CreateExpense(account, false, "Rent", 100.0, "2025-01-01", "2025-01-10", null, Frequency.Weeks, 1);

            account.Incomes.Add(income1);
            account.Incomes.Add(income2);
            account.Expenses.Add(expense);

            await context.AddAccountsAsync(account);

            var result = await context.Repository.GetAccountWithLinkedCountsOrDefaultAsync(account.RowId, Xunit.TestContext.Current.CancellationToken);

            result.ShouldNotBeNull();
            result.Account.RowId.ShouldBe(account.RowId);
            result.Account.Description.ShouldBe("Test Account");
            result.LinkedIncomes.ShouldBe(2);
            result.LinkedExpenses.ShouldBe(1);
        }

        [Fact]
        public async Task Should_Return_Zero_Counts_When_The_Account_Has_No_Links()
        {
            using var context = CreateTestContext();

            var account = EntityFactory.CreateAccount(context.Site, "Test Account", 1000.0);
            await context.AddAccountsAsync(account);

            var result = await context.Repository.GetAccountWithLinkedCountsOrDefaultAsync(account.RowId, Xunit.TestContext.Current.CancellationToken);

            result.ShouldNotBeNull();
            result.LinkedIncomes.ShouldBe(0);
            result.LinkedExpenses.ShouldBe(0);
        }

        [Fact]
        public async Task Should_Return_Null_When_The_Account_Does_Not_Exist()
        {
            using var context = CreateTestContext();

            var result = await context.Repository.GetAccountWithLinkedCountsOrDefaultAsync(Guid.NewGuid(), Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeNull();
        }
    }

    public class GetAllAccountsWithLinkedCountsAsync : AccountRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_All_Accounts_For_The_Current_Site_With_Linked_Counts()
        {
            using var context = CreateTestContext();

            var account1 = EntityFactory.CreateAccount(context.Site, "Account 1", 1000.0);
            var income = EntityFactory.CreateIncome(account1, false, "Salary", 100.0, "2025-01-10", null, Frequency.Weeks, 1);
            account1.Incomes.Add(income);

            var account2 = EntityFactory.CreateAccount(context.Site, "Account 2", 2000.0);
            var expense = EntityFactory.CreateExpense(account2, false, "Rent", 100.0, "2025-01-01", "2025-01-10", null, Frequency.Weeks, 1);
            account2.Expenses.Add(expense);

            var otherSiteAccount = EntityFactory.CreateAccount(context.OtherSite, "Other Site Account", 3000.0);

            await context.AddAccountsAsync(account1, account2, otherSiteAccount);

            var result = await context.Repository.GetAllAccountsWithLinkedCountsAsync(Xunit.TestContext.Current.CancellationToken);

            result.Length.ShouldBe(2);
            result.ShouldHaveValues(item => item.Account.Description, new[] { "Account 1", "Account 2" });

            var resultAccount1 = result.Single(item => item.Account.Description == "Account 1");
            resultAccount1.LinkedIncomes.ShouldBe(1);
            resultAccount1.LinkedExpenses.ShouldBe(0);

            var resultAccount2 = result.Single(item => item.Account.Description == "Account 2");
            resultAccount2.LinkedIncomes.ShouldBe(0);
            resultAccount2.LinkedExpenses.ShouldBe(1);
        }

        [Fact]
        public async Task Should_Return_Empty_Array_When_There_Are_No_Accounts_For_The_Current_Site()
        {
            using var context = CreateTestContext();

            var otherSiteAccount = EntityFactory.CreateAccount(context.OtherSite, "Other Site Account", 3000.0);
            await context.AddAccountsAsync(otherSiteAccount);

            var result = await context.Repository.GetAllAccountsWithLinkedCountsAsync(Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeEmpty();
        }
    }

    private static TestContext CreateTestContext()
    {
        var dbOptions = new DbContextOptionsBuilder<PotDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var site = EntityFactory.CreateSite();
        var user = EntityFactory.CreateUser(site);
        var otherSite = EntityFactory.CreateSite("Other Site", "Another Site");

        var currentUserContext = Substitute.For<ICurrentUserContext>();
        currentUserContext.UserRowId.Returns(user.RowId);

        var dbContext = new PotDbContext(dbOptions, currentUserContext);

        dbContext.Add(site);
        dbContext.Add(user);
        dbContext.Add(otherSite);
        dbContext.SaveChanges();

        var repository = new AccountRepository(dbContext);

        return new TestContext(dbContext, repository, site, otherSite);
    }
}
