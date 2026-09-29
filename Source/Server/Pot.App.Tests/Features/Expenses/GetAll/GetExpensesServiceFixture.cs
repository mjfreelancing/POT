using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Pot.App.Calculators;
using Pot.App.Concerns.Time;
using Pot.App.Features.Expenses.GetAll;
using Pot.Data.Entities;
using Pot.Data.Repositories.Expenses;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Expenses.GetAll;

public class GetExpensesServiceFixture : PotFixtureBase
{
    private static readonly DateOnly Today = new(2025, 1, 15);

    private readonly IExpenseRepository _expenseRepository = Substitute.For<IExpenseRepository>();
    private readonly ITimeProvider _timeProvider = Substitute.For<ITimeProvider>();

    public GetExpensesServiceFixture()
    {
        _timeProvider.GetLocalDateNow().Returns(Today);
    }

    [Fact]
    public async Task Should_Attribute_Accrued_And_Arrears_To_Every_Row_Across_Accounts()
    {
        var firstAccount = CreateAccount(accountId: 1, description: "Everyday");
        var secondAccount = CreateAccount(accountId: 2, description: "Bills");

        var dueToday = CreateExpense(firstAccount, amount: 100.0d, nextDue: "2025-01-15", accrualStart: "2025-01-15");
        var pastDueWeekly = CreateExpense(firstAccount, amount: 70.0d, nextDue: "2025-01-01", accrualStart: "2025-01-01",
            frequency: Frequency.Weeks);
        var ramping = CreateExpense(secondAccount, amount: 100.0d, nextDue: "2025-02-15", accrualStart: "2025-01-15");
        var withoutAccrualStart = CreateExpense(secondAccount, amount: 100.0d, nextDue: "2025-01-01", accrualStart: "2025-01-01");

        withoutAccrualStart.AccrualStart = null;

        _expenseRepository
            .GetAllExpensesAsync(Arg.Any<CancellationToken>())
            .Returns([dueToday, pastDueWeekly, ramping, withoutAccrualStart]);

        var outputs = await CreateService().GetAllExpensesAsync(TestContext.Current.CancellationToken);
        var byRowId = outputs.ToDictionary(output => output.RowId);

        byRowId.Count.ShouldBe(4, "the list carries a result for every row of the account");

        byRowId[dueToday.RowId].Accrued.ShouldBe(100.0d, "a bill due today accrues in full");
        byRowId[dueToday.RowId].Arrears.ShouldBe(0.0d);

        byRowId[pastDueWeekly.RowId].Accrued.ShouldBe(70.0d);
        byRowId[pastDueWeekly.RowId].Arrears.ShouldBe(140.0d, "two past-due weekly occurrences at 70 each");

        byRowId[ramping.RowId].Accrued.ShouldBe(0.0d);
        byRowId[ramping.RowId].Arrears.ShouldBe(0.0d);

        byRowId[withoutAccrualStart.RowId].Accrued.ShouldBe(0.0d, "a null accrual start contributes nothing to accrual");
        byRowId[withoutAccrualStart.RowId].Arrears.ShouldBe(100.0d, "but the past-due occurrence is still owed");
    }

    [Fact]
    public async Task Should_Pass_The_Expense_Fields_Through()
    {
        var account = CreateAccount(accountId: 1, description: "Everyday");
        var expense = CreateExpense(account, amount: 100.0d, nextDue: "2025-01-15", accrualStart: "2025-01-15");

        _expenseRepository
            .GetAllExpensesAsync(Arg.Any<CancellationToken>())
            .Returns([expense]);

        var output = (await CreateService().GetAllExpensesAsync(TestContext.Current.CancellationToken)).ShouldHaveSingleItem();

        output.RowId.ShouldBe(expense.RowId);
        output.Etag.ShouldBe(expense.Etag);
        output.Description.ShouldBe(expense.Description);
        output.Amount.ShouldBe(expense.Amount);
        output.NextDue.ShouldBe(expense.NextDue);
        output.Account.RowId.ShouldBe(account.RowId);
        output.Account.Description.ShouldBe(account.Description);
    }

    [Fact]
    public async Task Should_Return_No_Expenses_When_There_Are_None()
    {
        _expenseRepository
            .GetAllExpensesAsync(Arg.Any<CancellationToken>())
            .Returns([]);

        var outputs = await CreateService().GetAllExpensesAsync(TestContext.Current.CancellationToken);

        outputs.ShouldBeEmpty();
    }

    private GetExpensesService CreateService()
    {
        return new GetExpensesService(_expenseRepository, new AccrualCalculator(), _timeProvider,
            NullLogger<GetExpensesService>.Instance);
    }

    private static AccountEntity CreateAccount(int accountId, string description)
    {
        var account = EntityFactory.CreateAccount(EntityFactory.CreateSite(), description, 1000.0d);

        account.Id = accountId;

        return account;
    }

    private static ExpenseEntity CreateExpense(AccountEntity account, double amount, string nextDue, string accrualStart,
        Frequency? frequency = null)
    {
        return EntityFactory.CreateExpense(account, excludeFromCalc: false, "Bill", amount, accrualStart, nextDue,
            endDate: null, frequency ?? Frequency.Months, frequencyCount: 1);
    }
}
