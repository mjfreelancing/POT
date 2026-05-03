using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Concerns.Accruals;
using Pot.App.Features.Expenses.ToggleExclude;
using Pot.App.Features.Expenses.ToggleExclude.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Expenses;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Expenses.ToggleExclude;

public class ExcludeExpensesServiceFixture : PotFixtureBase
{
    private sealed class NoopScope : IDisposable
    {
        public void Dispose()
        {
        }
    }

    public class Constructor : ExcludeExpensesServiceFixture
    {
        private readonly IAccrualDirtyStateManager _accrualDirtyStateManagerFake;
        private readonly IPersistableExpenseRepository _expenseRepositoryFake;

        public Constructor()
        {
            _accrualDirtyStateManagerFake = Substitute.For<IAccrualDirtyStateManager>();
            _expenseRepositoryFake = Substitute.For<IPersistableExpenseRepository>();
        }

        [Fact]
        public void Should_Throw_When_AccrualDirtyStateManager_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var logger = Substitute.For<ILogger<ExcludeExpensesService>>();

                _ = new ExcludeExpensesService(null!, _expenseRepositoryFake, logger);
            });

            exception.ParamName.ShouldBe("accrualDirtyStateManager");
        }

        [Fact]
        public void Should_Throw_When_ExpenseRepository_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var logger = Substitute.For<ILogger<ExcludeExpensesService>>();

                _ = new ExcludeExpensesService(_accrualDirtyStateManagerFake, null!, logger);
            });

            exception.ParamName.ShouldBe("expenseRepository");
        }

        [Fact]
        public void Should_Throw_When_Logger_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                _ = new ExcludeExpensesService(_accrualDirtyStateManagerFake, _expenseRepositoryFake, null!);
            });

            exception.ParamName.ShouldBe("logger");
        }
    }

    public class ToggleExclusionAsync : ExcludeExpensesServiceFixture
    {
        private readonly IAccrualDirtyStateManager _accrualDirtyStateManagerFake;
        private readonly IPersistableExpenseRepository _expenseRepositoryFake;

        public ToggleExclusionAsync()
        {
            _accrualDirtyStateManagerFake = Substitute.For<IAccrualDirtyStateManager>();
            _expenseRepositoryFake = Substitute.For<IPersistableExpenseRepository>();
            _expenseRepositoryFake.WithTracking().Returns(new NoopScope());
        }

        /*
        TODO(logging): Re-enable when the replacement logging test framework is available.
        [Fact]
        public async Task Should_LogCall_When_Toggling_Expense_Exclusion()
        {
            var logger = Substitute.For<ILogger<ExcludeExpensesService>>();

            _expenseRepositoryFake
                .GetExpensesAsync(Arg.Any<Guid[]>(), Arg.Any<CancellationToken>())
                .Returns(new List<ExpenseEntity>());

            _accrualDirtyStateManagerFake
                .SetAccountsDirtyAsync(Arg.Any<IReadOnlyCollection<ExpenseEntity>>(), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            _expenseRepositoryFake
                .SaveAsync(Arg.Any<CancellationToken>())
                .Returns(1);

            var service = new ExcludeExpensesService(_accrualDirtyStateManagerFake, _expenseRepositoryFake, logger);
            var input = new Input
            {
                RowIds = []
            };

            var context = await logger.CaptureLogCallsAsync(async () =>
            {
                _ = await service.ToggleExclusionAsync(input, CancellationToken.None);
            });

            _ = context.ShouldLogCall<ExcludeExpensesService>(nameof(ExcludeExpensesService.ToggleExclusionAsync));
        }
        */

        [Fact]
        public async Task Should_Fail_When_One_Or_More_Expenses_Do_Not_Exist()
        {
            var logger = Substitute.For<ILogger<ExcludeExpensesService>>();

            var existingExpense = CreateExpense(accountId: 9, excluded: false);
            var missingRowId = Guid.NewGuid();

            _expenseRepositoryFake
                .GetExpensesAsync(Arg.Any<Guid[]>(), Arg.Any<CancellationToken>())
                .Returns([existingExpense]);

            var service = new ExcludeExpensesService(_accrualDirtyStateManagerFake, _expenseRepositoryFake, logger);

            var input = new Input
            {
                RowIds = [existingExpense.RowId, missingRowId]
            };

            var result = await service.ToggleExclusionAsync(input, CancellationToken.None);

            result.IsSuccess.ShouldBeFalse();

            await _accrualDirtyStateManagerFake
                .DidNotReceive()
                .SetAccountsDirtyAsync(Arg.Any<IReadOnlyCollection<ExpenseEntity>>(), Arg.Any<CancellationToken>());

            await _expenseRepositoryFake
                .DidNotReceive()
                .SaveAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Toggle_Exclusion_And_Mark_Dirty_When_All_Expenses_Exist()
        {
            var logger = Substitute.For<ILogger<ExcludeExpensesService>>();

            var firstExpense = CreateExpense(accountId: 10, excluded: false);
            var secondExpense = CreateExpense(accountId: 10, excluded: true);
            IReadOnlyCollection<ExpenseEntity>? markedExpenses = null;

            _expenseRepositoryFake
                .GetExpensesAsync(Arg.Any<Guid[]>(), Arg.Any<CancellationToken>())
                .Returns([firstExpense, secondExpense]);

            _accrualDirtyStateManagerFake
                .SetAccountsDirtyAsync(Arg.Do<IReadOnlyCollection<ExpenseEntity>>(expenses => markedExpenses = expenses), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            _expenseRepositoryFake
                .SaveAsync(Arg.Any<CancellationToken>())
                .Returns(1);

            var service = new ExcludeExpensesService(_accrualDirtyStateManagerFake, _expenseRepositoryFake, logger);

            var input = new Input
            {
                RowIds = [firstExpense.RowId, secondExpense.RowId]
            };

            var result = await service.ToggleExclusionAsync(input, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();

            firstExpense.ExcludeFromCalcs.ShouldBeTrue();
            secondExpense.ExcludeFromCalcs.ShouldBeFalse();

            markedExpenses.ShouldNotBeNull();
            markedExpenses.Count.ShouldBe(2);

            await _expenseRepositoryFake.Received(1).SaveAsync(Arg.Any<CancellationToken>());
        }
    }

    private static ExpenseEntity CreateExpense(int accountId, bool excluded)
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
            ExcludeFromCalcs = excluded,
            Description = "Expense",
            Amount = 45.0d,
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
