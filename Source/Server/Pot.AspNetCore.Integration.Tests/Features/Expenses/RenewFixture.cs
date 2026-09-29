using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Pot.AspNetCore.Integration.Tests.Host.Models;
using Pot.Shared.Enumerations;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace Pot.AspNetCore.Integration.Tests.Features.Expenses;

public class RenewFixture : IntegrationAuthFixtureBase
{
    private const string RenewPath = "/api/expenses/renew";
    private const double AccountBalance = 1000.0d;
    private const double BillAmount = 70.0d;
    private const int CycleDays = 30;

    private const string RowIdsProperty = "RowIds";

    private sealed class ValidationProblem
    {
        public ValidationError[] Errors { get; set; } = [];
    }

    private sealed class ValidationError
    {
        public string? PropertyName { get; set; }
        public string? ErrorMessage { get; set; }
    }

    [Fact]
    public async Task Should_Return_Unauthorized_When_Renewing_Without_Authentication()
    {
        using var client = CreateClient();

        var response = await client.RenewExpensesAsync([Guid.NewGuid()], GetSiteLocalDateToday(), RenewalMode.Overdue);

        await response.ShouldHaveStatusAsync(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_Return_MethodNotAllowed_When_Getting_The_Renew_Endpoint()
    {
        using var client = CreateClient();

        var response = await client.GetAsync(RenewPath, TestContext.Current.CancellationToken);

        await response.ShouldHaveStatusAsync(HttpStatusCode.MethodNotAllowed);
    }

    // An empty selection is rejected rather than treated as a no-op, because it can only be a caller mistake.
    [Fact]
    public async Task Should_Return_UnprocessableEntity_When_No_Expenses_Are_Selected()
    {
        var admin = await CreateAdminUserAsync("renew", "Renew User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var response = await client.RenewExpensesAsync([], GetSiteLocalDateToday(), RenewalMode.Overdue);

        await response.ShouldHaveStatusAsync(HttpStatusCode.UnprocessableEntity);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>(TestContext.Current.CancellationToken);

        problem.ShouldNotBeNull();
        problem!.Errors.Select(error => error.PropertyName).ShouldContain(RowIdsProperty);
    }

    // A selection the database cannot resolve is reported as a failure rather than a silent success.
    [Fact]
    public async Task Should_Return_UnprocessableEntity_When_An_Expense_Does_Not_Exist()
    {
        var admin = await CreateAdminUserAsync("renew", "Renew User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var response = await client.RenewExpensesAsync([Guid.NewGuid()], GetSiteLocalDateToday(), RenewalMode.Overdue);

        await response.ShouldHaveStatusAsync(HttpStatusCode.UnprocessableEntity);
    }

    // The overdue mode catches the schedule up to the first occurrence at or after the as-of date, which is what
    // discharges the arrears the read paths had reported.
    [Fact]
    public async Task Should_Catch_An_Overdue_Expense_Up_To_Today_And_Clear_Its_Arrears()
    {
        var admin = await CreateAdminUserAsync("renew", "Renew User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();
        var expenseRowId = await CreateBillAsync(client, today, today.AddDays(-45));

        var beforeRenewal = await client.GetExpenseAsync(expenseRowId);

        beforeRenewal.NextDue.ShouldBe(today.AddDays(-45));
        beforeRenewal.Arrears.ShouldBe(2 * BillAmount);

        var response = await client.RenewExpensesAsync([expenseRowId], today, RenewalMode.Overdue);

        await response.ShouldHaveStatusAsync(HttpStatusCode.OK);

        var afterRenewal = await client.GetExpenseAsync(expenseRowId);

        afterRenewal.NextDue.ShouldBe(today.AddDays(15), "two whole cycles are caught up, so the next occurrence is half a cycle ahead");
        afterRenewal.AccrualStart.ShouldBe(today.AddDays(-15), "the cycle in progress started at the last occurrence that passed");
        afterRenewal.Arrears.ShouldBe(0.0d, "nothing is past due once the schedule has been caught up");
        afterRenewal.Accrued.ShouldBe(BillAmount / 2, "catching up does not change where the cycle in progress is");
    }

    // The future mode advances exactly one cycle, so marking a future bill as paid does not skip a period.
    [Fact]
    public async Task Should_Advance_A_Future_Expense_By_Exactly_One_Cycle()
    {
        var admin = await CreateAdminUserAsync("renew", "Renew User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();
        var expenseRowId = await CreateBillAsync(client, today, today.AddDays(5));

        var response = await client.RenewExpensesAsync([expenseRowId], today, RenewalMode.Future);

        await response.ShouldHaveStatusAsync(HttpStatusCode.OK);

        var afterRenewal = await client.GetExpenseAsync(expenseRowId);

        afterRenewal.NextDue.ShouldBe(today.AddDays(35), "the future mode moves on by one cycle, not to the next occurrence after today");
        afterRenewal.Arrears.ShouldBe(0.0d);
    }

    private static async Task<Guid> CreateBillAsync(HttpClient client, DateOnly today, DateOnly nextDue)
    {
        var accountRowId = await client.CreateAccountAsync($"Renew Account {Guid.NewGuid():N}", AccountBalance);

        var expenseRequest = ExpenseRequestFactory.CreateBill(accountRowId, nextDue, Frequency.Days, CycleDays,
            BillAmount, AccrualPolicy.Automatic, accrualStart: today);

        return await client.CreateExpenseAsync(expenseRequest);
    }
}
