using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using Pot.App.Concerns.Accruals;
using Pot.App.Concerns.Time;
using Pot.App.Features.Expenses.Delete;
using Pot.Data;
using Pot.Data.Entities;
using Pot.Data.Repositories.AccountAccrual;
using Pot.Data.Repositories.Expenses;
using Pot.Shared;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Pot.TestUtils.Logging;
using Shouldly;

namespace Pot.App.Tests.Features.Expenses.Delete;

public class DeleteExpenseServiceFixture : PotFixtureBase
{
    private sealed class TestContext : IDisposable
    {
        public DeleteExpenseService Service { get; }
        public PotDbContext DbContext { get; }
        public SiteEntity Site { get; }
        public FakeLogCollector LogCollector { get; }

        public TestContext(DeleteExpenseService service, PotDbContext dbContext, SiteEntity site, FakeLogCollector logCollector)
        {
            Service = service;
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

        public ExpenseEntity AddExpense(AccountEntity account, string description, bool excludeFromCalcs = false)
        {
            var expense = EntityFactory.CreateExpense(
                account,
                excludeFromCalcs,
                description,
                amount: 100.0d,
                accrualStart: "2026-01-01",
                nextDue: "2026-02-01",
                endDate: null,
                frequency: Frequency.Months,
                frequencyCount: 1,
                accrualPolicy: AccrualPolicy.Automatic);

            DbContext.Expenses.Add(expense);
            DbContext.SaveChanges();

            return expense;
        }

        public AccountAccrualEntity AddAccountAccrual(AccountEntity account, bool isDirty)
        {
            var accountAccrual = new AccountAccrualEntity
            {
                AccountId = account.Id,
                AccruedIsDirty = isDirty,
                LastAccruedDate = new DateOnly(2026, 1, 1)
            };

            DbContext.AccountAccruals.Add(accountAccrual);
            DbContext.SaveChanges();

            return accountAccrual;
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }

    public class Constructor : DeleteExpenseServiceFixture
    {
        private readonly IAccrualDirtyStateManager _accrualDirtyStateManagerFake;
        private readonly IPersistableAccountAccrualRepository _accountAccrualRepositoryFake;
        private readonly IPersistableExpenseRepository _expenseRepositoryFake;
        private readonly ITimeProvider _timeProviderFake;

        public Constructor()
        {
            _accrualDirtyStateManagerFake = Substitute.For<IAccrualDirtyStateManager>();
            _accountAccrualRepositoryFake = Substitute.For<IPersistableAccountAccrualRepository>();
            _expenseRepositoryFake = Substitute.For<IPersistableExpenseRepository>();
            _timeProviderFake = Substitute.For<ITimeProvider>();
        }

        [Fact]
        public void Should_Throw_When_AccrualDirtyStateManager_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var logger = Substitute.For<ILogger<DeleteExpenseService>>();

                _ = new DeleteExpenseService(null!, _accountAccrualRepositoryFake, _expenseRepositoryFake, _timeProviderFake, logger);
            });

            exception.ParamName.ShouldBe("accrualDirtyStateManager");
        }

        [Fact]
        public void Should_Throw_When_AccountAccrualRepository_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var logger = Substitute.For<ILogger<DeleteExpenseService>>();

                _ = new DeleteExpenseService(_accrualDirtyStateManagerFake, null!, _expenseRepositoryFake, _timeProviderFake, logger);
            });

            exception.ParamName.ShouldBe("accountAccrualRepository");
        }

        [Fact]
        public void Should_Throw_When_ExpenseRepository_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var logger = Substitute.For<ILogger<DeleteExpenseService>>();

                _ = new DeleteExpenseService(_accrualDirtyStateManagerFake, _accountAccrualRepositoryFake, null!, _timeProviderFake, logger);
            });

            exception.ParamName.ShouldBe("expenseRepository");
        }

        [Fact]
        public void Should_Throw_When_TimeProvider_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var logger = Substitute.For<ILogger<DeleteExpenseService>>();

                _ = new DeleteExpenseService(_accrualDirtyStateManagerFake, _accountAccrualRepositoryFake, _expenseRepositoryFake, null!, logger);
            });

            exception.ParamName.ShouldBe("timeProvider");
        }

        [Fact]
        public void Should_Throw_When_Logger_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                _ = new DeleteExpenseService(_accrualDirtyStateManagerFake, _accountAccrualRepositoryFake, _expenseRepositoryFake, _timeProviderFake, null!);
            });

            exception.ParamName.ShouldBe("logger");
        }
    }

    public class DeleteExpenseAsync : DeleteExpenseServiceFixture
    {
        [Fact]
        public async Task Should_LogCall_When_Deleting_Expense()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Logging Account");
            var expense = context.AddExpense(account, "Logging Expense");

            _ = await context.Service.DeleteExpenseAsync(expense.RowId, CancellationToken.None);

            context.LogCollector.ShouldContainLogCall(
                category: typeof(DeleteExpenseService).FullName!,
                callerName: nameof(DeleteExpenseService.DeleteExpenseAsync),
                callerType: typeof(DeleteExpenseService));
        }

        [Fact]
        public async Task Should_Fail_When_Expense_Does_Not_Exist()
        {
            using var context = CreateTestContext();

            var result = await context.Service.DeleteExpenseAsync(Guid.NewGuid(), CancellationToken.None);

            result.IsSuccess.ShouldBeFalse();
        }

        [Fact]
        public async Task Should_Remove_AccountAccrual_When_Deleting_Last_Expense_For_Account()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Single Expense Account");
            var expense = context.AddExpense(account, "Only Expense");
            _ = context.AddAccountAccrual(account, isDirty: false);

            var result = await context.Service.DeleteExpenseAsync(expense.RowId, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            context.DbContext.Expenses.Count(item => item.Account.Id == account.Id).ShouldBe(0);
            context.DbContext.AccountAccruals.Count(item => item.AccountId == account.Id).ShouldBe(0);
        }

        [Fact]
        public async Task Should_Succeed_When_Deleting_Last_Expense_And_AccountAccrual_Is_Missing()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Single Expense Missing Accrual Account");
            var expense = context.AddExpense(account, "Only Expense Missing Accrual");

            var result = await context.Service.DeleteExpenseAsync(expense.RowId, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            context.DbContext.Expenses.Count(item => item.Account.Id == account.Id).ShouldBe(0);
            context.DbContext.AccountAccruals.Count(item => item.AccountId == account.Id).ShouldBe(0);
        }

        [Fact]
        public async Task Should_Mark_AccountAccrual_Dirty_When_Deleting_Impacting_Expense_And_Account_Has_Other_Expenses()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Multi Expense Account");
            var expenseToDelete = context.AddExpense(account, "Expense To Delete", excludeFromCalcs: false);
            _ = context.AddExpense(account, "Expense To Keep", excludeFromCalcs: false);
            _ = context.AddAccountAccrual(account, isDirty: false);

            var result = await context.Service.DeleteExpenseAsync(expenseToDelete.RowId, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            context.DbContext.Expenses.Count(item => item.Account.Id == account.Id).ShouldBe(1);

            var accountAccrual = context.DbContext.AccountAccruals.Single(item => item.AccountId == account.Id);

            accountAccrual.AccruedIsDirty.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_Not_Mark_AccountAccrual_Dirty_When_Deleting_NonImpacting_Expense_And_Account_Has_Other_Expenses()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Mixed Expense Account");
            var expenseToDelete = context.AddExpense(account, "Excluded Expense To Delete", excludeFromCalcs: true);
            _ = context.AddExpense(account, "Expense To Keep", excludeFromCalcs: false);
            _ = context.AddAccountAccrual(account, isDirty: false);

            var result = await context.Service.DeleteExpenseAsync(expenseToDelete.RowId, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            context.DbContext.Expenses.Count(item => item.Account.Id == account.Id).ShouldBe(1);

            var accountAccrual = context.DbContext.AccountAccruals.Single(item => item.AccountId == account.Id);

            accountAccrual.AccruedIsDirty.ShouldBeFalse();
        }

        [Fact]
        public async Task Should_Mark_AccountAccrual_Dirty_When_Deleting_Recurring_Ended_Expense_And_Account_Has_Other_Expenses()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Ended Expense Account");
            var expenseToDelete = context.AddExpense(account, "Ended Expense To Delete", excludeFromCalcs: false);
            expenseToDelete.EndDate = new DateOnly(2026, 1, 1);
            _ = context.AddExpense(account, "Expense To Keep", excludeFromCalcs: false);
            _ = context.AddAccountAccrual(account, isDirty: false);

            await context.DbContext.SaveChangesAsync();

            var result = await context.Service.DeleteExpenseAsync(expenseToDelete.RowId, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            context.DbContext.Expenses.Count(item => item.Account.Id == account.Id).ShouldBe(1);

            var accountAccrual = context.DbContext.AccountAccruals.Single(item => item.AccountId == account.Id);

            accountAccrual.AccruedIsDirty.ShouldBeTrue();
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

        var serviceLogCollector = new FakeLogCollector();
        var serviceLogger = new FakeLogger<DeleteExpenseService>(serviceLogCollector);
        var timeProvider = Substitute.For<ITimeProvider>();
        timeProvider.GetLocalDateNow().Returns(new DateOnly(2026, 2, 1));

        var accountAccrualRepositoryLogger = new FakeLogger<AccountAccrualRepository>();
        var accountAccrualMarkerLogger = new FakeLogger<AccrualDirtyStateManager>();

        var expenseRepository = new ExpenseRepository(dbContext);
        var accountAccrualRepository = new AccountAccrualRepository(dbContext, accountAccrualRepositoryLogger);
        var accrualDirtyStateManager = new AccrualDirtyStateManager(accountAccrualRepository, accountAccrualMarkerLogger);

        var service = new DeleteExpenseService(accrualDirtyStateManager, accountAccrualRepository, expenseRepository, timeProvider, serviceLogger);

        return new TestContext(service, dbContext, site, serviceLogCollector);
    }
}
