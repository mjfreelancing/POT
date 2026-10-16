using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Pot.AspNetCore.Integration.Tests.Host.Models;
using Pot.Shared.Enumerations;
using Shouldly;
using System.Net;

namespace Pot.AspNetCore.Integration.Tests.Features.Expenses;

public class DeleteFixture : IntegrationAuthFixtureBase
{
    [Fact]
    public async Task Should_Return_NoContent_When_Deleting_An_Expense()
    {
        var admin = await CreateAdminUserAsync("expenses", "Expenses User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();
        var accountRowId = await client.CreateAccountAsync($"Delete Account {Guid.NewGuid():N}", 1000.0d);
        var expense = ExpenseRequestFactory.CreateBill(accountRowId, today, Frequency.Days, 30, 70.0d,
            AccrualPolicy.Automatic, accrualStart: today);
        var expenseRowId = await client.CreateExpenseAsync(expense);
        var response = await client.DeleteAsync($"/api/expenses/{expenseRowId}", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        response.Content.Headers.ContentLength.ShouldBe(0);
    }
}
