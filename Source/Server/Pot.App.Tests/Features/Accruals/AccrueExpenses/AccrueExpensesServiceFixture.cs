using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using Pot.App.Calculators;
using Pot.App.Concerns.Accruals;
using Pot.App.Concerns.Time;
using Pot.App.Features.Accruals.AccrueExpenses;
using Pot.App.Features.Accruals.AccrueExpenses.Models;
using Pot.Data;
using Pot.Data.Entities;
using Pot.Data.Repositories.AccountAccrual;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Repositories.Expenses;
using Pot.Shared;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Pot.TestUtils.Logging;
using Shouldly;

namespace Pot.App.Tests.Features.Accruals.AccrueExpenses;

public class AccrueExpensesServiceFixture : PotFixtureBase
{
    private sealed class TestContext : IDisposable
    {
        public AccrueExpensesService Service { get; }
        public PotDbContext DbContext { get; }
        public SiteEntity Site { get; }
        public DateOnly LocalCurrentDate { get; }
        public FakeLogCollector LogCollector { get; }

        public TestContext(AccrueExpensesService service, PotDbContext dbContext, SiteEntity site, DateOnly localCurrentDate, FakeLogCollector logCollector)
        {
            Service = service;
            DbContext = dbContext;
            Site = site;
            LocalCurrentDate = localCurrentDate;
            LogCollector = logCollector;
        }

        public AccountEntity AddAccount(string description)
        {
            var account = EntityFactory.CreateAccount(Site, description, balance: 1000.0d);

            DbContext.Accounts.Add(account);
            DbContext.SaveChanges();

            return account;
        }

        public ExpenseEntity AddExpense(AccountEntity account, string description)
        {
            var expense = EntityFactory.CreateExpense(
                account,
                excludeFromCalc: false,
                description: description,
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

        public void AddAccountAccrual(AccountEntity account, bool isDirty, DateOnly? lastAccruedDate)
        {
            DbContext.AccountAccruals.Add(new AccountAccrualEntity
            {
                AccountId = account.Id,
                AccruedIsDirty = isDirty,
                LastAccruedDate = lastAccruedDate
            });

            DbContext.SaveChanges();
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }

    public class Constructor : AccrueExpensesServiceFixture
    {
        [Fact]
        public void Should_Throw_When_AccountAccrualDirtyMarker_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var accountRepository = Substitute.For<IAccountRepository>();
                var expenseRepository = Substitute.For<IPersistableExpenseRepository>();
                var calculator = Substitute.For<IAccrueExpenseCalculator>();
                var timeProvider = Substitute.For<ITimeProvider>();
                var logger = Substitute.For<ILogger<AccrueExpensesService>>();

                _ = new AccrueExpensesService(null!, accountRepository, expenseRepository, calculator, timeProvider, logger);
            });

            exception.ParamName.ShouldBe("accountAccrualDirtyMarker");
        }

        [Fact]
        public void Should_Throw_When_AccountRepository_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var accountAccrualDirtyMarker = Substitute.For<IAccountAccrualDirtyMarker>();
                var expenseRepository = Substitute.For<IPersistableExpenseRepository>();
                var calculator = Substitute.For<IAccrueExpenseCalculator>();
                var timeProvider = Substitute.For<ITimeProvider>();
                var logger = Substitute.For<ILogger<AccrueExpensesService>>();

                _ = new AccrueExpensesService(accountAccrualDirtyMarker, null!, expenseRepository, calculator, timeProvider, logger);
            });

            exception.ParamName.ShouldBe("accountRepository");
        }

        [Fact]
        public void Should_Throw_When_ExpenseRepository_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var accountAccrualDirtyMarker = Substitute.For<IAccountAccrualDirtyMarker>();
                var accountRepository = Substitute.For<IAccountRepository>();
                var calculator = Substitute.For<IAccrueExpenseCalculator>();
                var timeProvider = Substitute.For<ITimeProvider>();
                var logger = Substitute.For<ILogger<AccrueExpensesService>>();

                _ = new AccrueExpensesService(accountAccrualDirtyMarker, accountRepository, null!, calculator, timeProvider, logger);
            });

            exception.ParamName.ShouldBe("expenseRepository");
        }

        [Fact]
        public void Should_Throw_When_AccrueExpenseCalculator_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var accountAccrualDirtyMarker = Substitute.For<IAccountAccrualDirtyMarker>();
                var accountRepository = Substitute.For<IAccountRepository>();
                var expenseRepository = Substitute.For<IPersistableExpenseRepository>();
                var timeProvider = Substitute.For<ITimeProvider>();
                var logger = Substitute.For<ILogger<AccrueExpensesService>>();

                _ = new AccrueExpensesService(accountAccrualDirtyMarker, accountRepository, expenseRepository, null!, timeProvider, logger);
            });

            exception.ParamName.ShouldBe("accrueExpenseCalculator");
        }

        [Fact]
        public void Should_Throw_When_TimeProvider_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var accountAccrualDirtyMarker = Substitute.For<IAccountAccrualDirtyMarker>();
                var accountRepository = Substitute.For<IAccountRepository>();
                var expenseRepository = Substitute.For<IPersistableExpenseRepository>();
                var calculator = Substitute.For<IAccrueExpenseCalculator>();
                var logger = Substitute.For<ILogger<AccrueExpensesService>>();

                _ = new AccrueExpensesService(accountAccrualDirtyMarker, accountRepository, expenseRepository, calculator, null!, logger);
            });

            exception.ParamName.ShouldBe("timeProvider");
        }

        [Fact]
        public void Should_Throw_When_Logger_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var accountAccrualDirtyMarker = Substitute.For<IAccountAccrualDirtyMarker>();
                var accountRepository = Substitute.For<IAccountRepository>();
                var expenseRepository = Substitute.For<IPersistableExpenseRepository>();
                var calculator = Substitute.For<IAccrueExpenseCalculator>();
                var timeProvider = Substitute.For<ITimeProvider>();

                _ = new AccrueExpensesService(accountAccrualDirtyMarker, accountRepository, expenseRepository, calculator, timeProvider, null!);
            });

            exception.ParamName.ShouldBe("logger");
        }
    }

    public class AccrueAsync : AccrueExpensesServiceFixture
    {
        [Fact]
        public async Task Should_LogCall_When_Accruing_Expenses()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Accrue Logging Account");
            _ = context.AddExpense(account, "Accrue Logging Expense");

            var input = new Input
            {
                RowIds = [account.RowId]
            };

            _ = await context.Service.AccrueAsync(input, CancellationToken.None);

            context.LogCollector.ShouldContainLogCall(
                category: typeof(AccrueExpensesService).FullName!,
                callerName: nameof(AccrueExpensesService.AccrueAsync),
                callerType: typeof(AccrueExpensesService));
        }

        [Fact]
        public async Task Should_Clear_AccountAccrual_Dirty_And_Stamp_Date_When_AccountAccrual_Exists()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Accrue Existing AccountAccrual Account");
            var expense = context.AddExpense(account, "Accrue Existing AccountAccrual Expense");
            context.AddAccountAccrual(account, isDirty: true, lastAccruedDate: new DateOnly(2026, 4, 30));

            var input = new Input
            {
                RowIds = [account.RowId]
            };

            var result = await context.Service.AccrueAsync(input, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();

            var accountAccrual = context.DbContext.AccountAccruals.Single(item => item.AccountId == account.Id);

            accountAccrual.AccruedIsDirty.ShouldBeFalse();
            accountAccrual.LastAccruedDate.ShouldBe(context.LocalCurrentDate);

            expense.AccruedIsDirty.ShouldBeFalse();
            expense.LastAccruedUpdate.ShouldBe(context.LocalCurrentDate);
        }

        [Fact]
        public async Task Should_Add_AccountAccrual_Clean_Row_When_Missing()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Accrue Missing AccountAccrual Account");
            _ = context.AddExpense(account, "Accrue Missing AccountAccrual Expense");

            var input = new Input
            {
                RowIds = [account.RowId]
            };

            var result = await context.Service.AccrueAsync(input, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();

            var accountAccrual = context.DbContext.AccountAccruals.Single(item => item.AccountId == account.Id);

            accountAccrual.AccruedIsDirty.ShouldBeFalse();
            accountAccrual.LastAccruedDate.ShouldBe(context.LocalCurrentDate);
        }

        [Fact]
        public async Task Should_Fail_When_Account_Does_Not_Exist()
        {
            using var context = CreateTestContext();

            var input = new Input
            {
                RowIds = [Guid.NewGuid()]
            };

            var result = await context.Service.AccrueAsync(input, CancellationToken.None);

            result.IsSuccess.ShouldBeFalse();
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

        var localCurrentDate = new DateOnly(2026, 5, 10);

        var timeProvider = Substitute.For<ITimeProvider>();
        timeProvider.GetLocalDateNow().Returns(localCurrentDate);

        var expenseRepository = new ExpenseRepository(dbContext);
        var accountRepository = new AccountRepository(dbContext);

        var accountAccrualRepositoryLogger = new FakeLogger<AccountAccrualRepository>();
        var accountAccrualMarkerLogger = new FakeLogger<AccountAccrualDirtyMarker>();

        var accountAccrualRepository = new AccountAccrualRepository(dbContext, accountAccrualRepositoryLogger);
        var accountAccrualDirtyMarker = new AccountAccrualDirtyMarker(accountAccrualRepository, accountAccrualMarkerLogger);

        var accrueExpenseCalculator = new AccrueExpenseCalculator(timeProvider);

        var serviceLogCollector = new FakeLogCollector();
        var serviceLogger = new FakeLogger<AccrueExpensesService>(serviceLogCollector);

        var service = new AccrueExpensesService(
            accountAccrualDirtyMarker,
            accountRepository,
            expenseRepository,
            accrueExpenseCalculator,
            timeProvider,
            serviceLogger);

        return new TestContext(service, dbContext, site, localCurrentDate, serviceLogCollector);
    }
}
