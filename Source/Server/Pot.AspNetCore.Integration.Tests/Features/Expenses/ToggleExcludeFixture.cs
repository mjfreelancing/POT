using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Pot.AspNetCore.Integration.Tests.Host.Models;
using Pot.Shared.Enumerations;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace Pot.AspNetCore.Integration.Tests.Features.Expenses;

public class ToggleExcludeFixture : IntegrationAuthFixtureBase
{
    private const double AccountBalance = 1000.0d;
    private const double BillAmount = 70.0d;
    private const int CycleDays = 30;

    [Fact]
    public async Task Should_Toggle_Exclude_On_An_Expense()
    {
        var admin = await CreateAdminUserAsync("expenses", "Expenses User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();
        var accountRowId = await client.CreateAccountAsync($"Expense Account {Guid.NewGuid():N}", AccountBalance);
        var request = ExpenseRequestFactory.CreateBill(accountRowId, today.AddDays(CycleDays), Frequency.Days,
            CycleDays, BillAmount, AccrualPolicy.Automatic, accrualStart: today);
        var expenseRowId = await client.CreateExpenseAsync(request);

        var response = await ToggleExcludeAsync(client, [expenseRowId]);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var toggled = await client.GetExpenseAsync(expenseRowId);

        toggled.ExcludeFromCalcs.ShouldBeTrue("the toggle flips the flag it was created with");

        var secondResponse = await ToggleExcludeAsync(client, [expenseRowId]);

        secondResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var toggledBack = await client.GetExpenseAsync(expenseRowId);

        toggledBack.ExcludeFromCalcs.ShouldBeFalse("a second toggle flips it back");
    }

    [Fact]
    public async Task Should_Return_BadRequest_When_Toggling_With_No_Expenses_Selected()
    {
        var admin = await CreateAdminUserAsync("expenses", "Expenses User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var response = await ToggleExcludeAsync(client, []);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private static Task<HttpResponseMessage> ToggleExcludeAsync(HttpClient client, Guid[] expenseRowIds)
    {
        var request = new { RowIds = expenseRowIds };

        return client.PostAsJsonAsync("/api/expenses/toggleExclude", request, TestContext.Current.CancellationToken);
    }
}
