using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Features.Expenses.Delete;
using Pot.Data;
using Pot.Data.Entities;
using Pot.Data.Repositories.Expenses;
using Pot.Shared;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Expenses.Delete;

public class DeleteExpenseServiceFixture : PotFixtureBase
{
    private sealed class TestContext : IDisposable
    {
        public DeleteExpenseService Service { get; }
        public PotDbContext DbContext { get; }
        public SiteEntity Site { get; }
        private ILogger<DeleteExpenseService> Logger { get; }

        public TestContext(DeleteExpenseService service, PotDbContext dbContext, SiteEntity site, ILogger<DeleteExpenseService> logger)
        {
            Service = service;
            DbContext = dbContext;
            Site = site;
            Logger = logger;
        }

        /*
        TODO(logging): Re-enable when the replacement logging test framework is available.
        public Task<LoggerCallContext> CaptureLogCallsAsync(Func<Task> action)
        {
            return Logger.CaptureLogCallsAsync(action);
        }
        */

        public AccountEntity AddAccount(string description)
        {
            var account = EntityFactory.CreateAccount(Site, description, balance: 1000.0d);

            DbContext.Accounts.Add(account);
            DbContext.SaveChanges();

            return account;
        }

        public ExpenseEntity AddExpense(AccountEntity account, string description, bool excludeFromCalcs = false)
        {
            var expense = EntityFactory.CreateExpense(
                account,
                excludeFromCalcs,
                description,
                25.0d,
                "2026-01-01",
                "2026-02-01",
                null,
                Frequency.Months,
                frequencyCount: 1,
                accrualPolicy: AccrualPolicy.Automatic);

            DbContext.Expenses.Add(expense);
            DbContext.SaveChanges();

            return expense;
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }

    public class Constructor : DeleteExpenseServiceFixture
    {
        private readonly IPersistableExpenseRepository _expenseRepositoryFake;

        public Constructor()
        {
            _expenseRepositoryFake = Substitute.For<IPersistableExpenseRepository>();
        }

        [Fact]
        public void Should_Throw_When_ExpenseRepository_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var logger = Substitute.For<ILogger<DeleteExpenseService>>();

                _ = new DeleteExpenseService(null!, logger);
            });

            exception.ParamName.ShouldBe("expenseRepository");
        }

        [Fact]
        public void Should_Throw_When_Logger_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                _ = new DeleteExpenseService(_expenseRepositoryFake, null!);
            });

            exception.ParamName.ShouldBe("logger");
        }
    }

    public class DeleteExpenseAsync : DeleteExpenseServiceFixture
    {
        /*
        TODO(logging): Re-enable when the replacement logging test framework is available.
        [Fact]
        public async Task Should_LogCall_When_Deleting_Expense()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Logging Account");
            var expense = context.AddExpense(account, "Logging Expense");

            var logContext = await context.CaptureLogCallsAsync(async () =>
            {
                _ = await context.Service.DeleteExpenseAsync(expense.RowId, CancellationToken.None);
            });

            _ = logContext.ShouldLogCall<DeleteExpenseService>(nameof(DeleteExpenseService.DeleteExpenseAsync));
        }
        */

        [Fact]
        public async Task Should_Fail_When_Expense_Does_Not_Exist()
        {
            using var context = CreateTestContext();

            var result = await context.Service.DeleteExpenseAsync(Guid.NewGuid(), CancellationToken.None);

            result.IsSuccess.ShouldBeFalse();
        }

        /*
        TODO(logging): Re-enable when the replacement logging test framework is available.
        [Fact]
        public async Task Should_LogApiError_When_Expense_Does_Not_Exist()
        {
            using var context = CreateTestContext();

            var logContext = await context.CaptureLogCallsAsync(async () =>
            {
                _ = await context.Service.DeleteExpenseAsync(Guid.NewGuid(), CancellationToken.None);
            });

            _ = logContext.ShouldLogAtLevel<DeleteExpenseService>(LogLevel.Information, "The expense does not exist");
        }
        */

        [Fact]
        public async Task Should_Delete_Expense_When_It_Is_The_Only_Expense_For_The_Account()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Single Expense Account");
            var expense = context.AddExpense(account, "Only Expense");

            var result = await context.Service.DeleteExpenseAsync(expense.RowId, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            context.DbContext.Expenses.Count(item => item.Account.Id == account.Id).ShouldBe(0);
        }

        [Fact]
        public async Task Should_Delete_Expense_And_Keep_The_Accounts_Other_Expenses()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Multi Expense Account");
            var expenseToDelete = context.AddExpense(account, "Excluded Expense To Delete", excludeFromCalcs: true);
            var expenseToKeep = context.AddExpense(account, "Expense To Keep");

            var result = await context.Service.DeleteExpenseAsync(expenseToDelete.RowId, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();

            var remainingExpenses = context.DbContext.Expenses.Where(item => item.Account.Id == account.Id).ToArray();

            remainingExpenses.Length.ShouldBe(1);
            remainingExpenses[0].RowId.ShouldBe(expenseToKeep.RowId);
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

        var serviceLogger = Substitute.For<ILogger<DeleteExpenseService>>();

        var expenseRepository = new ExpenseRepository(dbContext);

        var service = new DeleteExpenseService(expenseRepository, serviceLogger);

        return new TestContext(service, dbContext, site, serviceLogger);
    }
}
