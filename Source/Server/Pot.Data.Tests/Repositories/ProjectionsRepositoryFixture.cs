using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Pot.Data.Entities;
using Pot.Data.Repositories.Projections;
using Pot.Shared;
using Pot.Shared.Enumerations;
using Pot.TestUtils;

namespace Pot.Data.Tests.Repositories;

public class ProjectionsRepositoryFixture : PotFixtureBase
{
    private sealed class TestContext : IDisposable
    {
        public PotDbContext DbContext { get; }
        public ProjectionsRepository Repository { get; }
        public SiteEntity Site { get; }
        public UserEntity User { get; }

        public TestContext(PotDbContext dbContext, ProjectionsRepository repository, SiteEntity site, UserEntity user)
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

        public Task<int> AddAccountsAsync(params AccountEntity[] accounts)
        {
            foreach (var account in accounts)
            {
                DbContext.Add(account);
            }

            return DbContext.SaveChangesAsync();
        }

        public Task<List<AccountEntity>> GetAllAccountsAsync(CancellationToken cancellationToken = default)
        {
            return Repository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(cancellationToken);
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }

    public class GetAllAccountsWithCandidateIncomesAndExpensesAsync : ProjectionsRepositoryFixture
    {
        public class SiteFiltering : GetAllAccountsWithCandidateIncomesAndExpensesAsync
        {
            [Fact]
            public async Task Should_Return_Accounts_For_Current_Site()
            {
                using var context = CreateTestContext();

                var account = EntityFactory.CreateAccount(context.Site, "Test Account", 1000.0);
                await context.AddAccountAsync(account);

                var result = await context.GetAllAccountsAsync();

                result.Should().HaveCount(1);
                result[0].Description.Should().Be("Test Account");
                result[0].Balance.Should().Be(1000.0);
            }

            [Fact]
            public async Task Should_Not_Return_Accounts_From_Different_Site()
            {
                using var context = CreateTestContextWithMultipleSites(out var otherSite, out _);

                var ownAccount = EntityFactory.CreateAccount(context.Site, "Own Account", 1000.0);
                var otherAccount = EntityFactory.CreateAccount(otherSite, "Other Account", 2000.0);

                await context.AddAccountsAsync(ownAccount, otherAccount);

                var result = await context.GetAllAccountsAsync();

                result.Should().HaveCount(1);
                result[0].Description.Should().Be("Own Account");
            }

            [Fact]
            public async Task Should_Return_Multiple_Accounts_From_Same_Site()
            {
                using var context = CreateTestContext();

                var account1 = EntityFactory.CreateAccount(context.Site, "Account 1", 1000.0);
                var account2 = EntityFactory.CreateAccount(context.Site, "Account 2", 2000.0);
                var account3 = EntityFactory.CreateAccount(context.Site, "Account 3", 3000.0);

                await context.AddAccountsAsync(account1, account2, account3);

                var result = await context.GetAllAccountsAsync();

                result.Should().HaveCount(3);
                result.Select(a => a.Description).Should().BeEquivalentTo("Account 1", "Account 2", "Account 3");
            }

            [Fact]
            public async Task Should_Return_Empty_List_When_No_Accounts_For_Site()
            {
                using var context = CreateTestContext();

                var result = await context.GetAllAccountsAsync();

                result.Should().BeEmpty();
            }

            [Fact]
            public async Task Should_Return_Empty_List_When_All_Accounts_Belong_To_Other_Sites()
            {
                using var context = CreateTestContextWithMultipleSites(out var otherSite, out _);

                var otherAccount1 = EntityFactory.CreateAccount(otherSite, "Other Account 1", 1000.0);
                var otherAccount2 = EntityFactory.CreateAccount(otherSite, "Other Account 2", 2000.0);

                await context.AddAccountsAsync(otherAccount1, otherAccount2);

                var result = await context.GetAllAccountsAsync();

                result.Should().BeEmpty();
            }
        }

        public class ExpenseFiltering : GetAllAccountsWithCandidateIncomesAndExpensesAsync
        {
            [Fact]
            public async Task Should_Include_Expenses_With_ExcludeFromCalcs_False()
            {
                using var context = CreateTestContext();

                var account = EntityFactory.CreateAccount(context.Site, "Test Account", 1000.0);
                var expense1 = EntityFactory.CreateExpense(account, false, "Included Expense 1", 100.0, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);
                var expense2 = EntityFactory.CreateExpense(account, false, "Included Expense 2", 200.0, "2025-01-01", "2025-01-25", null, Frequency.Months, 1);

                account.Expenses.Add(expense1);
                account.Expenses.Add(expense2);

                await context.AddAccountAsync(account);

                var result = await context.GetAllAccountsAsync();

                result.Should().HaveCount(1);
                result[0].Expenses.Should().HaveCount(2);
                result[0].Expenses.Select(e => e.Description).Should().BeEquivalentTo("Included Expense 1", "Included Expense 2");
            }

            [Fact]
            public async Task Should_Exclude_Expenses_With_ExcludeFromCalcs_True()
            {
                using var context = CreateTestContext();

                var account = EntityFactory.CreateAccount(context.Site, "Test Account", 1000.0);
                var excludedExpense = EntityFactory.CreateExpense(account, true, "Excluded Expense", 500.0, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);

                account.Expenses.Add(excludedExpense);

                await context.AddAccountAsync(account);

                var result = await context.GetAllAccountsAsync();

                result.Should().HaveCount(1);
                result[0].Expenses.Should().BeEmpty();
            }

            [Fact]
            public async Task Should_Handle_Mixed_Excluded_And_Included_Expenses()
            {
                using var context = CreateTestContext();

                var account = EntityFactory.CreateAccount(context.Site, "Test Account", 1000.0);
                var included1 = EntityFactory.CreateExpense(account, false, "Included 1", 100.0, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);
                var excluded1 = EntityFactory.CreateExpense(account, true, "Excluded 1", 200.0, "2025-01-01", "2025-01-25", null, Frequency.Months, 1);
                var included2 = EntityFactory.CreateExpense(account, false, "Included 2", 300.0, "2025-01-01", "2025-01-20", null, Frequency.Months, 1);
                var excluded2 = EntityFactory.CreateExpense(account, true, "Excluded 2", 400.0, "2025-01-01", "2025-01-15", null, Frequency.Months, 1);

                account.Expenses.Add(included1);
                account.Expenses.Add(excluded1);
                account.Expenses.Add(included2);
                account.Expenses.Add(excluded2);

                await context.AddAccountAsync(account);

                var result = await context.GetAllAccountsAsync();

                result.Should().HaveCount(1);
                result[0].Expenses.Should().HaveCount(2);
                result[0].Expenses.Select(e => e.Description).Should().BeEquivalentTo("Included 1", "Included 2");
            }
        }

        public class IncomeFiltering : GetAllAccountsWithCandidateIncomesAndExpensesAsync
        {
            [Fact]
            public async Task Should_Include_Incomes_With_ExcludeFromCalcs_False()
            {
                using var context = CreateTestContext();

                var account = EntityFactory.CreateAccount(context.Site, "Test Account", 1000.0);
                var income1 = EntityFactory.CreateIncome(account, false, "Included Income 1", 3000.0, "2025-01-31", null, Frequency.Months, 1);
                var income2 = EntityFactory.CreateIncome(account, false, "Included Income 2", 1500.0, "2025-01-15", null, Frequency.Weeks, 2);

                account.Incomes.Add(income1);
                account.Incomes.Add(income2);

                await context.AddAccountAsync(account);

                var result = await context.GetAllAccountsAsync();

                result.Should().HaveCount(1);
                result[0].Incomes.Should().HaveCount(2);
                result[0].Incomes.Select(i => i.Description).Should().BeEquivalentTo("Included Income 1", "Included Income 2");
            }

            [Fact]
            public async Task Should_Exclude_Incomes_With_ExcludeFromCalcs_True()
            {
                using var context = CreateTestContext();

                var account = EntityFactory.CreateAccount(context.Site, "Test Account", 1000.0);
                var excludedIncome = EntityFactory.CreateIncome(account, true, "Excluded Income", 5000.0, "2025-01-31", null, Frequency.Months, 1);

                account.Incomes.Add(excludedIncome);

                await context.AddAccountAsync(account);

                var result = await context.GetAllAccountsAsync();

                result.Should().HaveCount(1);
                result[0].Incomes.Should().BeEmpty();
            }

            [Fact]
            public async Task Should_Handle_Mixed_Excluded_And_Included_Incomes()
            {
                using var context = CreateTestContext();

                var account = EntityFactory.CreateAccount(context.Site, "Test Account", 1000.0);
                var included1 = EntityFactory.CreateIncome(account, false, "Included 1", 3000.0, "2025-01-31", null, Frequency.Months, 1);
                var excluded1 = EntityFactory.CreateIncome(account, true, "Excluded 1", 1000.0, "2025-01-25", null, Frequency.Months, 1);
                var included2 = EntityFactory.CreateIncome(account, false, "Included 2", 1500.0, "2025-01-15", null, Frequency.Weeks, 2);
                var excluded2 = EntityFactory.CreateIncome(account, true, "Excluded 2", 500.0, "2025-01-20", null, Frequency.Weeks, 1);

                account.Incomes.Add(included1);
                account.Incomes.Add(excluded1);
                account.Incomes.Add(included2);
                account.Incomes.Add(excluded2);

                await context.AddAccountAsync(account);

                var result = await context.GetAllAccountsAsync();

                result.Should().HaveCount(1);
                result[0].Incomes.Should().HaveCount(2);
                result[0].Incomes.Select(i => i.Description).Should().BeEquivalentTo("Included 1", "Included 2");
            }
        }

        public class CombinedScenarios : GetAllAccountsWithCandidateIncomesAndExpensesAsync
        {
            [Fact]
            public async Task Should_Filter_By_Site_And_ExcludeFromCalcs_Together()
            {
                using var context = CreateTestContextWithMultipleSites(out var otherSite, out _);

                var ownAccount = EntityFactory.CreateAccount(context.Site, "Own Account", 1000.0);
                var ownIncludedExpense = EntityFactory.CreateExpense(ownAccount, false, "Own Included Expense", 100.0, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);
                var ownExcludedExpense = EntityFactory.CreateExpense(ownAccount, true, "Own Excluded Expense", 200.0, "2025-01-01", "2025-01-25", null, Frequency.Months, 1);
                var ownIncludedIncome = EntityFactory.CreateIncome(ownAccount, false, "Own Included Income", 3000.0, "2025-01-31", null, Frequency.Months, 1);
                var ownExcludedIncome = EntityFactory.CreateIncome(ownAccount, true, "Own Excluded Income", 1000.0, "2025-01-25", null, Frequency.Months, 1);

                ownAccount.Expenses.Add(ownIncludedExpense);
                ownAccount.Expenses.Add(ownExcludedExpense);
                ownAccount.Incomes.Add(ownIncludedIncome);
                ownAccount.Incomes.Add(ownExcludedIncome);

                var otherAccount = EntityFactory.CreateAccount(otherSite, "Other Account", 2000.0);
                var otherIncludedExpense = EntityFactory.CreateExpense(otherAccount, false, "Other Included Expense", 300.0, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);
                var otherIncludedIncome = EntityFactory.CreateIncome(otherAccount, false, "Other Included Income", 4000.0, "2025-01-31", null, Frequency.Months, 1);

                otherAccount.Expenses.Add(otherIncludedExpense);
                otherAccount.Incomes.Add(otherIncludedIncome);

                await context.AddAccountsAsync(ownAccount, otherAccount);

                var result = await context.GetAllAccountsAsync();

                result.Should().HaveCount(1);
                result[0].Description.Should().Be("Own Account");
                result[0].Expenses.Should().HaveCount(1);
                result[0].Expenses.First().Description.Should().Be("Own Included Expense");
                result[0].Incomes.Should().HaveCount(1);
                result[0].Incomes.First().Description.Should().Be("Own Included Income");
            }

            [Fact]
            public async Task Should_Return_Account_With_No_Incomes_Or_Expenses()
            {
                using var context = CreateTestContext();

                var account = EntityFactory.CreateAccount(context.Site, "Empty Account", 1000.0);

                await context.AddAccountAsync(account);

                var result = await context.GetAllAccountsAsync();

                result.Should().HaveCount(1);
                result[0].Description.Should().Be("Empty Account");
                result[0].Expenses.Should().BeEmpty();
                result[0].Incomes.Should().BeEmpty();
            }

            [Fact]
            public async Task Should_Return_Account_With_Only_Excluded_Items_As_Empty_Collections()
            {
                using var context = CreateTestContext();

                var account = EntityFactory.CreateAccount(context.Site, "Account With Excluded Items", 1000.0);
                var excludedExpense = EntityFactory.CreateExpense(account, true, "Excluded Expense", 100.0, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);
                var excludedIncome = EntityFactory.CreateIncome(account, true, "Excluded Income", 3000.0, "2025-01-31", null, Frequency.Months, 1);

                account.Expenses.Add(excludedExpense);
                account.Incomes.Add(excludedIncome);

                await context.AddAccountAsync(account);

                var result = await context.GetAllAccountsAsync();

                result.Should().HaveCount(1);
                result[0].Description.Should().Be("Account With Excluded Items");
                result[0].Expenses.Should().BeEmpty();
                result[0].Incomes.Should().BeEmpty();
            }

            [Fact]
            public async Task Should_Handle_Multiple_Accounts_With_Mixed_Filtering()
            {
                using var context = CreateTestContextWithMultipleSites(out var otherSite, out _);

                var account1 = EntityFactory.CreateAccount(context.Site, "Account 1", 1000.0);
                var account1Expense = EntityFactory.CreateExpense(account1, false, "Account 1 Expense", 100.0, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);
                account1.Expenses.Add(account1Expense);

                var account2 = EntityFactory.CreateAccount(context.Site, "Account 2", 2000.0);
                var account2Income = EntityFactory.CreateIncome(account2, false, "Account 2 Income", 3000.0, "2025-01-31", null, Frequency.Months, 1);
                var account2ExcludedExpense = EntityFactory.CreateExpense(account2, true, "Account 2 Excluded Expense", 200.0, "2025-01-01", "2025-01-25", null, Frequency.Months, 1);
                account2.Incomes.Add(account2Income);
                account2.Expenses.Add(account2ExcludedExpense);

                var account3 = EntityFactory.CreateAccount(context.Site, "Account 3", 3000.0);

                var otherAccount = EntityFactory.CreateAccount(otherSite, "Other Account", 5000.0);
                var otherExpense = EntityFactory.CreateExpense(otherAccount, false, "Other Expense", 500.0, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);
                otherAccount.Expenses.Add(otherExpense);

                await context.AddAccountsAsync(account1, account2, account3, otherAccount);

                var result = await context.GetAllAccountsAsync();

                result.Should().HaveCount(3);
                result.Select(a => a.Description).Should().BeEquivalentTo("Account 1", "Account 2", "Account 3");

                var resultAccount1 = result.First(a => a.Description == "Account 1");
                resultAccount1.Expenses.Should().HaveCount(1);
                resultAccount1.Expenses.First().Description.Should().Be("Account 1 Expense");
                resultAccount1.Incomes.Should().BeEmpty();

                var resultAccount2 = result.First(a => a.Description == "Account 2");
                resultAccount2.Incomes.Should().HaveCount(1);
                resultAccount2.Incomes.First().Description.Should().Be("Account 2 Income");
                resultAccount2.Expenses.Should().BeEmpty();

                var resultAccount3 = result.First(a => a.Description == "Account 3");
                resultAccount3.Expenses.Should().BeEmpty();
                resultAccount3.Incomes.Should().BeEmpty();
            }
        }

        public class EdgeCases : GetAllAccountsWithCandidateIncomesAndExpensesAsync
        {
            [Fact]
            public async Task Should_Return_Empty_List_When_Database_Empty()
            {
                using var context = CreateTestContext();

                var result = await context.GetAllAccountsAsync();

                result.Should().BeEmpty();
            }

            [Fact]
            public async Task Should_Handle_Cancellation_Token()
            {
                using var context = CreateTestContext();

                var account = EntityFactory.CreateAccount(context.Site, "Test Account", 1000.0);

                await context.AddAccountAsync(account);

                using var cts = new CancellationTokenSource();
                cts.Cancel();

                await Invoking(async () => await context.GetAllAccountsAsync(cts.Token))
                    .Should()
                    .ThrowAsync<OperationCanceledException>();
            }

            [Fact]
            public async Task Should_Handle_Account_With_Many_Incomes_And_Expenses()
            {
                using var context = CreateTestContext();

                var account = EntityFactory.CreateAccount(context.Site, "Busy Account", 10000.0);

                for (int i = 0; i < 10; i++)
                {
                    var includedExpense = EntityFactory.CreateExpense(account, false, $"Included Expense {i}", 100.0, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);
                    var excludedExpense = EntityFactory.CreateExpense(account, true, $"Excluded Expense {i}", 200.0, "2025-01-01", "2025-01-25", null, Frequency.Months, 1);
                    account.Expenses.Add(includedExpense);
                    account.Expenses.Add(excludedExpense);
                }

                for (int i = 0; i < 10; i++)
                {
                    var includedIncome = EntityFactory.CreateIncome(account, false, $"Included Income {i}", 1000.0, "2025-01-31", null, Frequency.Months, 1);
                    var excludedIncome = EntityFactory.CreateIncome(account, true, $"Excluded Income {i}", 500.0, "2025-01-25", null, Frequency.Months, 1);
                    account.Incomes.Add(includedIncome);
                    account.Incomes.Add(excludedIncome);
                }

                await context.AddAccountAsync(account);

                var result = await context.GetAllAccountsAsync();

                result.Should().HaveCount(1);
                result[0].Expenses.Should().HaveCount(10);
                result[0].Incomes.Should().HaveCount(10);
                result[0].Expenses.Should().AllSatisfy(e => e.Description.Should().StartWith("Included Expense"));
                result[0].Incomes.Should().AllSatisfy(i => i.Description.Should().StartWith("Included Income"));
            }
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

        var repository = new ProjectionsRepository(dbContext);

        return new TestContext(dbContext, repository, site, user);
    }

    private static TestContext CreateTestContextWithMultipleSites(out SiteEntity otherSite, out UserEntity otherUser)
    {
        var dbOptions = new DbContextOptionsBuilder<PotDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var site1 = EntityFactory.CreateSite("Site 1", "First Site");
        var user1 = EntityFactory.CreateUser(site1, "user1", "user1@example.com", "User 1");

        var site2 = EntityFactory.CreateSite("Site 2", "Second Site");
        var user2 = EntityFactory.CreateUser(site1, "user2", "user2@example.com", "User 2");

        var currentUserContext = Substitute.For<ICurrentUserContext>();
        currentUserContext.UserRowId.Returns(user1.RowId);

        var dbContext = new PotDbContext(dbOptions, currentUserContext);

        dbContext.Add(site1);
        dbContext.Add(user1);
        dbContext.Add(site2);
        dbContext.Add(user2);
        dbContext.SaveChanges();

        var repository = new ProjectionsRepository(dbContext);

        otherSite = site2;
        otherUser = user2;

        return new TestContext(dbContext, repository, site1, user1);
    }
}
