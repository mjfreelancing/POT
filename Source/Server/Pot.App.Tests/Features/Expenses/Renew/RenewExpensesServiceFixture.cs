using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using Pot.App.Calculators;
using Pot.App.Concerns.Accruals;
using Pot.App.Features.Expenses.Renew;
using Pot.App.Features.Expenses.Renew.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Expenses;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Pot.TestUtils.Logging;
using Shouldly;

namespace Pot.App.Tests.Features.Expenses.Renew;

public class RenewExpensesServiceFixture : PotFixtureBase
{
    private sealed class NoopScope : IDisposable
    {
        public void Dispose()
        {
        }
    }

    public class Constructor : RenewExpensesServiceFixture
    {
        private readonly IAccrualDirtyStateManager _accrualDirtyStateManagerFake;
        private readonly IPersistableExpenseRepository _expenseRepositoryFake;
        private readonly IExpenseRenewalCalculator _renewalCalculatorFake;

        public Constructor()
        {
            _accrualDirtyStateManagerFake = Substitute.For<IAccrualDirtyStateManager>();
            _expenseRepositoryFake = Substitute.For<IPersistableExpenseRepository>();
            _renewalCalculatorFake = Substitute.For<IExpenseRenewalCalculator>();
        }

        [Fact]
        public void Should_Throw_When_AccrualDirtyStateManager_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var logger = Substitute.For<ILogger<RenewExpensesService>>();

                _ = new RenewExpensesService(null!, _expenseRepositoryFake, _renewalCalculatorFake, logger);
            });

            exception.ParamName.ShouldBe("accrualDirtyStateManager");
        }

        [Fact]
        public void Should_Throw_When_ExpenseRepository_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var logger = Substitute.For<ILogger<RenewExpensesService>>();

                _ = new RenewExpensesService(_accrualDirtyStateManagerFake, null!, _renewalCalculatorFake, logger);
            });

            exception.ParamName.ShouldBe("expenseRepository");
        }

        [Fact]
        public void Should_Throw_When_RenewalCalculator_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var logger = Substitute.For<ILogger<RenewExpensesService>>();

                _ = new RenewExpensesService(_accrualDirtyStateManagerFake, _expenseRepositoryFake, null!, logger);
            });

            exception.ParamName.ShouldBe("renewalCalculator");
        }

        [Fact]
        public void Should_Throw_When_Logger_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                _ = new RenewExpensesService(_accrualDirtyStateManagerFake, _expenseRepositoryFake, _renewalCalculatorFake, null!);
            });

            exception.ParamName.ShouldBe("logger");
        }
    }

    public class RenewAsync : RenewExpensesServiceFixture
    {
        private readonly IAccrualDirtyStateManager _accrualDirtyStateManagerFake;
        private readonly IPersistableExpenseRepository _expenseRepositoryFake;
        private readonly IExpenseRenewalCalculator _renewalCalculatorFake;

        public RenewAsync()
        {
            _accrualDirtyStateManagerFake = Substitute.For<IAccrualDirtyStateManager>();
            _expenseRepositoryFake = Substitute.For<IPersistableExpenseRepository>();
            _renewalCalculatorFake = Substitute.For<IExpenseRenewalCalculator>();
            _expenseRepositoryFake.WithTracking().Returns(new NoopScope());
        }

        [Fact]
        public async Task Should_LogCall_When_Renewing_Expenses()
        {
            var logCollector = new FakeLogCollector();
            var logger = new FakeLogger<RenewExpensesService>(logCollector);

            _expenseRepositoryFake
                .GetExpensesAsync(Arg.Any<Guid[]>(), Arg.Any<CancellationToken>())
                .Returns(new List<ExpenseEntity>());

            _accrualDirtyStateManagerFake
                .SetAccountsDirtyAsync(Arg.Any<IReadOnlyCollection<ExpenseEntity>>(), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            _expenseRepositoryFake
                .SaveAsync(Arg.Any<CancellationToken>())
                .Returns(1);

            var service = new RenewExpensesService(_accrualDirtyStateManagerFake, _expenseRepositoryFake, _renewalCalculatorFake, logger);
            var input = new Input
            {
                RowIds = [],
                AsOfDate = new DateOnly(2026, 4, 24),
                Mode = RenewalMode.Overdue
            };

            _ = await service.RenewAsync(input, CancellationToken.None);

            logCollector.ShouldContainLogCall(
                category: typeof(RenewExpensesService).FullName!,
                callerName: nameof(RenewExpensesService.RenewAsync),
                callerType: typeof(RenewExpensesService));
        }

        [Fact]
        public async Task Should_Fail_When_One_Or_More_Expenses_Do_Not_Exist()
        {
            var logger = Substitute.For<ILogger<RenewExpensesService>>();

            var existingExpense = CreateExpense(accountId: 5);
            var missingRowId = Guid.NewGuid();

            _expenseRepositoryFake
                .GetExpensesAsync(Arg.Any<Guid[]>(), Arg.Any<CancellationToken>())
                .Returns([existingExpense]);

            var service = new RenewExpensesService(_accrualDirtyStateManagerFake, _expenseRepositoryFake, _renewalCalculatorFake, logger);

            var input = new Input
            {
                RowIds = [existingExpense.RowId, missingRowId],
                AsOfDate = new DateOnly(2026, 4, 24),
                Mode = RenewalMode.Overdue
            };

            var result = await service.RenewAsync(input, CancellationToken.None);

            result.IsSuccess.ShouldBeFalse();

            _renewalCalculatorFake
                .DidNotReceive()
                .Renew(Arg.Any<IEnumerable<ExpenseEntity>>(), Arg.Any<RenewalMode>(), Arg.Any<DateOnly>());

            await _expenseRepositoryFake
                .DidNotReceive()
                .SaveAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Renew_Expenses_And_Mark_Changed_Ones_Dirty()
        {
            var logger = Substitute.For<ILogger<RenewExpensesService>>();

            var changedExpense = CreateExpense(accountId: 6);
            var unchangedExpense = CreateExpense(accountId: 7);
            IReadOnlyCollection<ExpenseEntity>? markedExpenses = null;

            _expenseRepositoryFake
                .GetExpensesAsync(Arg.Any<Guid[]>(), Arg.Any<CancellationToken>())
                .Returns([changedExpense, unchangedExpense]);

            _renewalCalculatorFake
                .When(calculator => calculator.Renew(Arg.Any<IEnumerable<ExpenseEntity>>(), Arg.Any<RenewalMode>(), Arg.Any<DateOnly>()))
                .Do(_ => changedExpense.NextDue = changedExpense.NextDue.AddMonths(1));

            _accrualDirtyStateManagerFake
                .SetAccountsDirtyAsync(Arg.Do<IReadOnlyCollection<ExpenseEntity>>(expenses => markedExpenses = expenses), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            _expenseRepositoryFake
                .SaveAsync(Arg.Any<CancellationToken>())
                .Returns(1);

            var service = new RenewExpensesService(_accrualDirtyStateManagerFake, _expenseRepositoryFake, _renewalCalculatorFake, logger);

            var input = new Input
            {
                RowIds = [changedExpense.RowId, unchangedExpense.RowId],
                AsOfDate = new DateOnly(2026, 4, 24),
                Mode = RenewalMode.Overdue
            };

            var result = await service.RenewAsync(input, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();

            markedExpenses.ShouldNotBeNull();
            markedExpenses.Select(item => item.RowId).ToArray().ShouldBe([changedExpense.RowId]);

            await _expenseRepositoryFake.Received(1).SaveAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Still_Call_Mark_Dirty_With_Empty_Set_When_No_Expenses_Are_Changed()
        {
            var logger = Substitute.For<ILogger<RenewExpensesService>>();

            var unchangedExpense = CreateExpense(accountId: 8);
            IReadOnlyCollection<ExpenseEntity>? markedExpenses = null;

            _expenseRepositoryFake
                .GetExpensesAsync(Arg.Any<Guid[]>(), Arg.Any<CancellationToken>())
                .Returns([unchangedExpense]);

            _accrualDirtyStateManagerFake
                .SetAccountsDirtyAsync(Arg.Do<IReadOnlyCollection<ExpenseEntity>>(expenses => markedExpenses = expenses), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            _expenseRepositoryFake
                .SaveAsync(Arg.Any<CancellationToken>())
                .Returns(1);

            var service = new RenewExpensesService(_accrualDirtyStateManagerFake, _expenseRepositoryFake, _renewalCalculatorFake, logger);

            var input = new Input
            {
                RowIds = [unchangedExpense.RowId],
                AsOfDate = new DateOnly(2026, 4, 24),
                Mode = RenewalMode.Future
            };

            var result = await service.RenewAsync(input, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            markedExpenses.ShouldNotBeNull();
            markedExpenses.ShouldBeEmpty();
        }
    }

    private static ExpenseEntity CreateExpense(int accountId)
    {
        var site = EntityFactory.CreateSite();
        var account = new AccountEntity
        {
            Id = accountId,
            RowId = Guid.NewGuid(),
            Bsb = "123-456",
            Number = "12345678",
            Description = $"Account-{accountId}",
            Balance = 1000.0d,
            Reserved = 0.0d,
            Site = site,
            Expenses = [],
            Incomes = []
        };

        return new ExpenseEntity
        {
            RowId = Guid.NewGuid(),
            ExcludeFromCalcs = false,
            Description = "Recurring expense",
            Amount = 70.0d,
            AccrualStart = new DateOnly(2026, 4, 1),
            NextDue = new DateOnly(2026, 5, 1),
            EndDate = null,
            AccrualPolicy = AccrualPolicy.Automatic,
            Frequency = Frequency.Months,
            FrequencyCount = 1,
            Account = account
        };
    }
}
