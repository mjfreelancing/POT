using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Pot.AspNetCore.Integration.Tests.Host.Models;
using Pot.Shared.Enumerations;
using Shouldly;

namespace Pot.AspNetCore.Integration.Tests.Features.Accounts;

public class AccrualDerivationFixture : IntegrationAuthFixtureBase
{
    private const double AccountBalance = 1000.0d;
    private const double AccountReserved = 100.0d;
    private const double BillAmount = 70.0d;
    private const int CycleDays = 30;

    // The list and single-item responses are separate types, so both are asserted wherever the figures must agree.
    [Fact]
    public async Task Should_Reduce_Available_By_Total_Committed_When_An_Expense_Is_Past_Due()
    {
        var admin = await CreateAdminUserAsync("accounts", "Accounts User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();
        var accountRowId = await client.CreateAccountAsync(CreateAccountDescription(), AccountBalance, AccountReserved);

        // A one-time bill that fell due yesterday has nothing left to accrue towards, so its whole obligation
        // sits in arrears rather than in accrual.
        var expenseRequest = ExpenseRequestFactory.CreateBill(accountRowId, today.AddDays(-1), Frequency.OneTime, 0,
            BillAmount, AccrualPolicy.Automatic, accrualStart: today);

        await client.CreateExpenseAsync(expenseRequest);

        var listed = (await client.GetAccountsAsync()).ShouldHaveSingleItem();

        AssertPastDueAccount(listed);

        var fetched = await client.GetAccountAsync(accountRowId);

        fetched.RowId.ShouldBe(accountRowId);

        AssertPastDueAccount(fetched);
    }

    // An occurrence due today is the current bill: it accrues in full and is not arrears, which is the boundary
    // that stops a due-today bill being counted twice.
    [Fact]
    public async Task Should_Not_Treat_An_Expense_Due_Today_As_Arrears()
    {
        var admin = await CreateAdminUserAsync("accounts", "Accounts User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();
        var accountRowId = await client.CreateAccountAsync(CreateAccountDescription(), AccountBalance, AccountReserved);
        var expenseRequest = ExpenseRequestFactory.CreateBill(accountRowId, today, Frequency.Days, CycleDays, BillAmount,
            AccrualPolicy.Automatic, accrualStart: today);

        await client.CreateExpenseAsync(expenseRequest);

        var account = (await client.GetAccountsAsync()).ShouldHaveSingleItem();

        account.TotalArrears.ShouldBe(0.0d);
        account.TotalExpenseAccrued.ShouldBe(BillAmount);
        account.TotalCommitted.ShouldBe(BillAmount);
        account.Available.ShouldBe(AccountBalance - AccountReserved - BillAmount);
    }

    // The values are derived on read, so an edit is reflected by the next read with nothing in between. Asserting
    // it here is what proves the account figures are no longer whatever the last accrual run happened to store.
    [Fact]
    public async Task Should_Reflect_An_Expense_Edit_Without_An_Intervening_Accrual_Call()
    {
        const double editedAmount = BillAmount * 2;

        var admin = await CreateAdminUserAsync("accounts", "Accounts User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();
        var accountRowId = await client.CreateAccountAsync(CreateAccountDescription(), AccountBalance, AccountReserved);

        // Due forty-five days ago on a thirty-day cycle, so two whole cycles are past due and the third is
        // exactly half way through its ramp.
        var expenseRequest = ExpenseRequestFactory.CreateBill(accountRowId, today.AddDays(-45), Frequency.Days, CycleDays,
            BillAmount, AccrualPolicy.Automatic, accrualStart: today);

        var expenseRowId = await client.CreateExpenseAsync(expenseRequest);

        var beforeEdit = (await client.GetAccountsAsync()).ShouldHaveSingleItem();

        beforeEdit.TotalArrears.ShouldBe(2 * BillAmount);
        beforeEdit.TotalExpenseAccrued.ShouldBe(BillAmount / 2);
        beforeEdit.TotalCommitted.ShouldBe(2 * BillAmount + BillAmount / 2);
        beforeEdit.Available.ShouldBe(AccountBalance - AccountReserved - beforeEdit.TotalCommitted);

        var expense = await client.GetExpenseAsync(expenseRowId);
        var updateRequest = expenseRequest with { Etag = expense.Etag, Amount = editedAmount };

        await client.UpdateExpenseAsync(expenseRowId, updateRequest);

        var afterEdit = (await client.GetAccountsAsync()).ShouldHaveSingleItem();

        afterEdit.TotalArrears.ShouldBe(2 * editedAmount);
        afterEdit.TotalExpenseAccrued.ShouldBe(editedAmount / 2);
        afterEdit.TotalCommitted.ShouldBe(2 * editedAmount + editedAmount / 2);
        afterEdit.Available.ShouldBe(AccountBalance - AccountReserved - afterEdit.TotalCommitted);
    }

    private static void AssertPastDueAccount(AccountResponse account)
    {
        account.TotalExpenseAccrued.ShouldBe(0.0d);
        account.TotalArrears.ShouldBe(BillAmount);
        account.TotalCommitted.ShouldBe(BillAmount);

        // The obligation is the whole of what the account is short, so Available falls by the arrears as well as
        // by the reserved amount.
        account.Available.ShouldBe(AccountBalance - AccountReserved - BillAmount);
    }

    private static string CreateAccountDescription()
    {
        return $"Accrual Account {Guid.NewGuid():N}";
    }
}
