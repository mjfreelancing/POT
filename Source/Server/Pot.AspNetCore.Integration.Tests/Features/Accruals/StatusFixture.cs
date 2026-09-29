using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Pot.AspNetCore.Integration.Tests.Host.Models;
using Pot.Shared.Enumerations;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Pot.AspNetCore.Integration.Tests.Features.Accruals;

public class StatusFixture : IntegrationAuthFixtureBase
{
    private const string StatusPath = "/api/accruals/status";
    private const double AccountBalance = 1000.0d;
    private const double BillAmount = 70.0d;
    private const int CycleDays = 30;

    // The status payload carries renewal requirements and nothing else: accrual is derived on read now, so there
    // is no accrual field left to keep in step with the calculation.
    private static readonly string[] ExpectedStatusProperties = ["expenseRenewalsRequired", "incomeRenewalsRequired"];

    [Fact]
    public async Task Should_Return_Unauthorized_When_Requesting_Status_Without_Authentication()
    {
        using var client = CreateClient();

        var response = await client.GetAsync($"{StatusPath}?AccountRowIds={Guid.NewGuid()}", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_Return_MethodNotAllowed_When_Posting_Status()
    {
        using var client = CreateClient();

        var response = await client.PostAsync($"{StatusPath}?AccountRowIds={Guid.NewGuid()}", null, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task Should_Report_Only_Renewal_Requirements()
    {
        var admin = await CreateAdminUserAsync("status", "Status User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();
        var accountRowId = await client.CreateAccountAsync($"Status Account {Guid.NewGuid():N}", AccountBalance);

        var overdueRequest = ExpenseRequestFactory.CreateBill(accountRowId, today.AddDays(-CycleDays), Frequency.Days,
            CycleDays, BillAmount, AccrualPolicy.Automatic, accrualStart: today.AddDays(-CycleDays));

        var overdueRowId = await client.CreateExpenseAsync(overdueRequest);

        var futureRequest = ExpenseRequestFactory.CreateBill(accountRowId, today.AddDays(CycleDays), Frequency.Days,
            CycleDays, BillAmount, AccrualPolicy.Automatic, accrualStart: today);

        var futureRowId = await client.CreateExpenseAsync(futureRequest);

        var response = await client.GetAsync($"{StatusPath}?AccountRowIds={accountRowId}", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        var propertyNames = payload.EnumerateObject().Select(property => property.Name).ToArray();

        propertyNames.ShouldBe(ExpectedStatusProperties, "status reports renewal requirements only");

        var expenseRenewals = payload.GetProperty("expenseRenewalsRequired").EnumerateArray()
            .Select(value => value.GetGuid())
            .ToArray();

        expenseRenewals.ShouldBe([overdueRowId], "only the expense that is due on or before today needs renewing");
        expenseRenewals.ShouldNotContain(futureRowId);

        payload.GetProperty("incomeRenewalsRequired").GetArrayLength().ShouldBe(0);
    }
}
