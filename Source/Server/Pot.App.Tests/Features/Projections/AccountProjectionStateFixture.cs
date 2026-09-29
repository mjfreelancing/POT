using Pot.App.Calculators;
using Pot.App.Features.Projections;
using Pot.Data.Entities;
using Pot.Shared.Enumerations;
using Pot.Shared.Models;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Projections;

public class AccountProjectionStateFixture : PotFixtureBase
{
    private static readonly DateOnly FoldDate = new(2025, 1, 29);

    [Fact]
    public void Should_Position_Every_Row_On_Its_Persisted_Schedule()
    {
        var account = CreateAccount();

        var state = new AccountProjectionState(account);

        state.ExpenseRows.Length.ShouldBe(1);
        state.IncomeRows.Length.ShouldBe(1);

        var expenseRow = state.ExpenseRows[0];

        expenseRow.Facts.RowId.ShouldBe(account.Expenses.First().RowId);
        expenseRow.Facts.Amount.ShouldBe(70.0d);
        expenseRow.Facts.Frequency.ShouldBe(Frequency.Months);
        expenseRow.Facts.FrequencyCount.ShouldBe(1);
        expenseRow.Facts.NextDue.ShouldBe(new DateOnly(2025, 1, 20));
        expenseRow.Facts.AccrualStart.ShouldBe(new DateOnly(2025, 1, 1));
        expenseRow.Cursor.ShouldBe(new AccrualCursor(expenseRow.Facts.NextDue, expenseRow.Facts.AccrualStart));

        // The position pairs the immutable facts with the cursor the loop folds, so the calculation is measured
        // against the schedule as it stands rather than a mutated entity.
        var positions = state.CreatePositions().ToArray();

        positions.Length.ShouldBe(1);
        positions[0].Facts.ShouldBeSameAs(expenseRow.Facts);
        positions[0].Cursor.ShouldBe(expenseRow.Cursor);
    }

    [Fact]
    public void Should_Start_The_Running_Balance_From_The_Account_And_Keep_It_Local()
    {
        var account = CreateAccount();

        var state = new AccountProjectionState(account);

        state.RunningBalance.ShouldBe(1000.0d);

        state.RecordCashMovement(incomeReceived: 200.0d, expensesPaid: 70.0d);

        state.RunningBalance.ShouldBe(1130.0d);
        account.Balance.ShouldBe(1000.0d, "the projection never writes the account");
    }

    [Fact]
    public void Should_Hold_The_First_Arrears_For_The_Whole_Window()
    {
        var account = CreateAccount();

        var state = new AccountProjectionState(account);

        state.Arrears.ShouldBe(0.0d);

        state.HoldArrears(210.0d);

        state.Arrears.ShouldBe(210.0d);

        // A later day measures the folded schedule, whose own view of the debt is zero. Releasing the held value
        // would read as the forecast repaying the pre-existing debt.
        state.HoldArrears(0.0d);

        state.Arrears.ShouldBe(210.0d);
    }

    [Fact]
    public void Should_Fold_Every_Row_Without_Touching_The_Facts()
    {
        var account = CreateAccount();
        var state = new AccountProjectionState(account);

        state.FoldRenewals(new ExpenseRenewalFold(), new IncomeRenewalFold(), FoldDate);

        var expenseRow = state.ExpenseRows[0];

        // Monthly from 2025-01-20: the 20th is settled, so the cursor advances one period and accrual restarts
        // from the occurrence that was settled.
        expenseRow.Cursor.NextDue.ShouldBe(new DateOnly(2025, 2, 20));
        expenseRow.Cursor.AccrualStart.ShouldBe(new DateOnly(2025, 1, 20));
        expenseRow.Facts.NextDue.ShouldBe(new DateOnly(2025, 1, 20), "folding moves the cursor, not the facts");
        expenseRow.Facts.AccrualStart.ShouldBe(new DateOnly(2025, 1, 1));

        state.IncomeRows[0].Schedule.NextDue.ShouldBe(new DateOnly(2025, 2, 10));
    }

    private static AccountEntity CreateAccount()
    {
        var site = EntityFactory.CreateSite();
        var account = EntityFactory.CreateAccount(site, "Visa", 1000.0d, reserved: 250.0d);

        account.Id = 42;

        var expense = EntityFactory.CreateExpense(account, false, "Bill", 70.0d, "2025-01-01", "2025-01-20", null, Frequency.Months, 1);
        var income = EntityFactory.CreateIncome(account, false, "Salary", 2000.0d, "2025-01-10", null, Frequency.Months, 1);

        account.Expenses.Add(expense);
        account.Incomes.Add(income);

        return account;
    }
}
