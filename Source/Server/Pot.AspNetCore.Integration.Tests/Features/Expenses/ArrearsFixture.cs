using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Pot.AspNetCore.Integration.Tests.Host.Models;
using Pot.Shared.Enumerations;
using Shouldly;

namespace Pot.AspNetCore.Integration.Tests.Features.Expenses;

public class ArrearsFixture : IntegrationAuthFixtureBase
{
    private const double AccountBalance = 1000.0d;
    private const double BillAmount = 70.0d;
    private const int CycleDays = 30;

    // Due forty-five days ago on a thirty-day cycle: two whole cycles are past due and the third is exactly half
    // way through its ramp, so the two figures are told apart by more than their magnitude.
    private const int DaysPastDue = -45;

    [Fact]
    public async Task Should_Expose_Accrued_And_Arrears_For_A_Past_Due_Expense()
    {
        var admin = await CreateAdminUserAsync("expenses", "Expenses User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();
        var accountRowId = await client.CreateAccountAsync($"Arrears Account {Guid.NewGuid():N}", AccountBalance);
        var expenseRequest = ExpenseRequestFactory.CreateBill(accountRowId, today.AddDays(DaysPastDue), Frequency.Days,
            CycleDays, BillAmount, AccrualPolicy.Automatic, accrualStart: today);

        var expenseRowId = await client.CreateExpenseAsync(expenseRequest);

        // The list and single-item responses are separate types, so both are asserted.
        var listed = (await client.GetExpensesAsync()).ShouldHaveSingleItem();

        listed.RowId.ShouldBe(expenseRowId);

        AssertPastDueExpense(listed);

        var fetched = await client.GetExpenseAsync(expenseRowId);

        AssertPastDueExpense(fetched);
    }

    // Being behind on a bill is not the same question as whether the bill accrues, and the schedule is what
    // decides, so a non-accruing bill still carries its arrears.
    [Fact]
    public async Task Should_Report_Arrears_For_A_Non_Accruing_Past_Due_Expense()
    {
        var admin = await CreateAdminUserAsync("expenses", "Expenses User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();
        var accountRowId = await client.CreateAccountAsync($"Arrears Account {Guid.NewGuid():N}", AccountBalance);
        var expenseRequest = ExpenseRequestFactory.CreateBill(accountRowId, today.AddDays(DaysPastDue), Frequency.Days,
            CycleDays, BillAmount, AccrualPolicy.None, accrualStart: null);

        var expenseRowId = await client.CreateExpenseAsync(expenseRequest);

        var expense = await client.GetExpenseAsync(expenseRowId);

        expense.Accrued.ShouldBe(0.0d);
        expense.Arrears.ShouldBe(2 * BillAmount);
    }

    // A one-time expense does not renew, so however long ago it fell due it carries at most the one occurrence.
    [Fact]
    public async Task Should_Carry_One_Occurrence_For_A_One_Time_Expense_Past_Due()
    {
        var admin = await CreateAdminUserAsync("expenses", "Expenses User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();
        var accountRowId = await client.CreateAccountAsync($"Arrears Account {Guid.NewGuid():N}", AccountBalance);
        var expenseRequest = ExpenseRequestFactory.CreateBill(accountRowId, today.AddDays(DaysPastDue), Frequency.OneTime,
            0, BillAmount, AccrualPolicy.None, accrualStart: null);

        var expenseRowId = await client.CreateExpenseAsync(expenseRequest);

        var expense = await client.GetExpenseAsync(expenseRowId);

        expense.Accrued.ShouldBe(0.0d);
        expense.Arrears.ShouldBe(BillAmount);
    }

    // The per-row figures are derived on read, so an edit is reflected by the next read with nothing in between.
    [Fact]
    public async Task Should_Reflect_An_Expense_Edit_Without_An_Intervening_Accrual_Call()
    {
        const double editedAmount = BillAmount * 2;

        var admin = await CreateAdminUserAsync("expenses", "Expenses User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();
        var accountRowId = await client.CreateAccountAsync($"Arrears Account {Guid.NewGuid():N}", AccountBalance);
        var expenseRequest = ExpenseRequestFactory.CreateBill(accountRowId, today.AddDays(DaysPastDue), Frequency.Days,
            CycleDays, BillAmount, AccrualPolicy.Automatic, accrualStart: today);

        var expenseRowId = await client.CreateExpenseAsync(expenseRequest);

        var beforeEdit = await client.GetExpenseAsync(expenseRowId);

        beforeEdit.Accrued.ShouldBe(BillAmount / 2);
        beforeEdit.Arrears.ShouldBe(2 * BillAmount);

        var updateRequest = expenseRequest with { Etag = beforeEdit.Etag, Amount = editedAmount };

        await client.UpdateExpenseAsync(expenseRowId, updateRequest);

        var afterEdit = await client.GetExpenseAsync(expenseRowId);

        afterEdit.Accrued.ShouldBe(editedAmount / 2);
        afterEdit.Arrears.ShouldBe(2 * editedAmount);
    }

    private static void AssertPastDueExpense(ExpenseResponse expense)
    {
        expense.Accrued.ShouldBe(BillAmount / 2);
        expense.Arrears.ShouldBe(2 * BillAmount);
    }
}
