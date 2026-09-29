using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Pot.App.Calculators;
using Pot.App.Concerns.Time;
using Pot.App.Errors;
using Pot.App.Features.Expenses.Get;
using Pot.Data.Entities;
using Pot.Data.Repositories.Expenses;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Expenses.Get;

public class GetExpenseServiceFixture : PotFixtureBase
{
    private static readonly DateOnly Today = new(2025, 1, 15);

    private readonly IPersistableExpenseRepository _expenseRepository = Substitute.For<IPersistableExpenseRepository>();
    private readonly ITimeProvider _timeProvider = Substitute.For<ITimeProvider>();

    public GetExpenseServiceFixture()
    {
        _timeProvider.GetLocalDateNow().Returns(Today);
    }

    [Fact]
    public async Task Should_Fail_When_The_Expense_Does_Not_Exist()
    {
        var expenseRowId = Guid.NewGuid();

        _expenseRepository
            .GetExpenseOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((ExpenseEntity?)null);

        var result = await CreateService().GetExpenseAsync(expenseRowId, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeFalse();
        _ = result.Error.ShouldBeOfType<ApiDetailError>();
    }

    [Fact]
    public async Task Should_Compose_The_Accrued_And_Arrears_For_The_Row()
    {
        var account = EntityFactory.CreateAccount(EntityFactory.CreateSite(), "Everyday", 1000.0d);
        var expense = EntityFactory.CreateExpense(account, excludeFromCalc: false, "Bill", 70.0d, "2025-01-01", "2025-01-01",
            endDate: null, Frequency.Weeks, frequencyCount: 1);

        _expenseRepository
            .GetExpenseOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(expense);

        var result = await CreateService().GetExpenseAsync(expense.RowId, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value!.Accrued.ShouldBe(70.0d, "the cycle in progress is due today, so it accrues in full");
        result.Value!.Arrears.ShouldBe(140.0d, "two past-due weekly occurrences at 70 each");
    }

    private GetExpenseService CreateService()
    {
        return new GetExpenseService(_expenseRepository, new AccrualCalculator(), _timeProvider,
            NullLogger<GetExpenseService>.Instance);
    }
}
