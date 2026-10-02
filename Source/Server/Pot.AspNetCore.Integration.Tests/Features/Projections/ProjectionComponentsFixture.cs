using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Pot.AspNetCore.Integration.Tests.Host.Models;
using Pot.Shared.Enumerations;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Pot.AspNetCore.Integration.Tests.Features.Projections;

public class ProjectionComponentsFixture : IntegrationAuthFixtureBase
{
    private const string ProjectionsPath = "/api/projections";
    private const double AccountBalance = 1000.0d;
    private const double BillAmount = 100.0d;
    private const int CycleDays = 30;
    private const int WindowDays = 5;

    private sealed class ProjectionResponse
    {
        public AccountProjection[] Accounts { get; set; } = [];
        public DateProjection[] Global { get; set; } = [];
    }

    private sealed class AccountProjection
    {
        public DateProjection[] Dates { get; set; } = [];
    }

    private sealed class DateProjection
    {
        public DateOnly Date { get; set; }
        public double Balance { get; set; }
        public double Reserved { get; set; }
        public double Arrears { get; set; }
        public double UnpaidAccrual { get; set; }

        // The value a consumer gets by netting the published components out of the balance.
        public double Composed => Balance - Reserved - UnpaidAccrual - Arrears;
    }

    [Fact]
    public async Task Should_Publish_The_Components_And_Not_Available()
    {
        var admin = await CreateAdminUserAsync("projections", "Projections User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();

        await SeedPaymentDayAsync(client, today, AccrualPolicy.Automatic);

        var path = BuildPath(today, today.AddDays(WindowDays - 1));
        var response = await client.GetAsync(path, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        using var document = JsonDocument.Parse(content);

        var accountDates = document.RootElement.GetProperty("accounts")[0].GetProperty("dates").EnumerateArray();
        var globalDates = document.RootElement.GetProperty("global").EnumerateArray();

        foreach (var date in accountDates.Concat(globalDates))
        {
            date.TryGetProperty("balance", out _).ShouldBeTrue();
            date.TryGetProperty("reserved", out _).ShouldBeTrue();
            date.TryGetProperty("arrears", out _).ShouldBeTrue();
            date.TryGetProperty("unpaidAccrual", out _).ShouldBeTrue();
            date.TryGetProperty("available", out _).ShouldBeFalse();
        }
    }

    // Today is a payment day, so the forecast has already paid the bill. The accrual measured before the schedule
    // advances still counts it, so only the part its payment settles may be removed, otherwise the composed value
    // credits the day for a payment that was never accrued and reads above the balance the same forecast produced.
    [Fact]
    public async Task Should_Compose_To_The_Balance_On_A_Payment_Day_For_An_Accruing_Bill()
    {
        var admin = await CreateAdminUserAsync("projections", "Projections User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();

        await SeedPaymentDayAsync(client, today, AccrualPolicy.Automatic);

        var projection = await GetProjectionAsync(client, today, today.AddDays(WindowDays - 1));
        var account = projection.Accounts.ShouldHaveSingleItem();

        account.Dates.Length.ShouldBe(WindowDays);

        var paymentDay = account.Dates[0];

        paymentDay.Date.ShouldBe(today);
        paymentDay.Balance.ShouldBe(AccountBalance - BillAmount);
        paymentDay.UnpaidAccrual.ShouldBe(0.0d, 0.001d);
        paymentDay.Composed.ShouldBe(AccountBalance - BillAmount, 0.001d);
    }

    // A bill that does not accrue is still paid on its due date, so the day must not be credited for an accrual
    // it never made. The settled amount exists to cancel a double count, not to hand back money.
    [Fact]
    public async Task Should_Compose_To_The_Balance_On_A_Payment_Day_For_A_Non_Accruing_Bill()
    {
        var admin = await CreateAdminUserAsync("projections", "Projections User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();

        await SeedPaymentDayAsync(client, today, AccrualPolicy.None);

        var projection = await GetProjectionAsync(client, today, today.AddDays(WindowDays - 1));
        var paymentDay = projection.Accounts.ShouldHaveSingleItem().Dates[0];

        paymentDay.Balance.ShouldBe(AccountBalance - BillAmount);
        paymentDay.UnpaidAccrual.ShouldBe(0.0d, 0.001d);
        paymentDay.Composed.ShouldBe(AccountBalance - BillAmount, 0.001d);
    }

    // An overdraft is real, so it is reported rather than floored at zero.
    [Fact]
    public async Task Should_Not_Mask_A_Temporary_Overdraft_On_A_Payment_Day()
    {
        var admin = await CreateAdminUserAsync("projections", "Projections User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();
        var accountRowId = await client.CreateAccountAsync($"Overdrawn Account {Guid.NewGuid():N}", 0.0d);

        var expenseRequest = ExpenseRequestFactory.CreateBill(accountRowId, today, Frequency.Days, CycleDays, BillAmount,
            AccrualPolicy.Automatic, accrualStart: today);

        await client.CreateExpenseAsync(expenseRequest);

        var projection = await GetProjectionAsync(client, today, today.AddDays(WindowDays - 1));
        var paymentDay = projection.Accounts.ShouldHaveSingleItem().Dates[0];

        paymentDay.Balance.ShouldBe(-BillAmount);
        paymentDay.Composed.ShouldBe(-BillAmount, 0.001d);
    }

    [Fact]
    public async Task Should_Not_Publish_A_Negative_Component()
    {
        var admin = await CreateAdminUserAsync("projections", "Projections User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();

        await SeedPaymentDayAsync(client, today, AccrualPolicy.Automatic);

        var projection = await GetProjectionAsync(client, today, today.AddDays(WindowDays - 1));
        var account = projection.Accounts.ShouldHaveSingleItem();

        AssertNoNegativeComponent(account.Dates);
        AssertNoNegativeComponent(projection.Global);
    }

    private static void AssertNoNegativeComponent(IEnumerable<DateProjection> dates)
    {
        foreach (var date in dates)
        {
            date.Reserved.ShouldBeGreaterThanOrEqualTo(0.0d, $"reserved on {date.Date:yyyy-MM-dd}");
            date.Arrears.ShouldBeGreaterThanOrEqualTo(0.0d, $"arrears on {date.Date:yyyy-MM-dd}");
            date.UnpaidAccrual.ShouldBeGreaterThanOrEqualTo(-1e-9, $"unpaid accrual on {date.Date:yyyy-MM-dd}");
        }
    }

    private static async Task SeedPaymentDayAsync(HttpClient client, DateOnly today, AccrualPolicy accrualPolicy)
    {
        var accountRowId = await client.CreateAccountAsync($"Payment Day Account {Guid.NewGuid():N}", AccountBalance);
        var accrualStart = accrualPolicy == AccrualPolicy.None ? (DateOnly?)null : today;

        var expenseRequest = ExpenseRequestFactory.CreateBill(accountRowId, today, Frequency.Days, CycleDays, BillAmount,
            accrualPolicy, accrualStart);

        await client.CreateExpenseAsync(expenseRequest);
    }

    private static string BuildPath(DateOnly startDate, DateOnly endDate)
    {
        return $"{ProjectionsPath}?StartDate={startDate:yyyy-MM-dd}&EndDate={endDate:yyyy-MM-dd}";
    }

    private static async Task<ProjectionResponse> GetProjectionAsync(HttpClient client, DateOnly startDate, DateOnly endDate)
    {
        var path = BuildPath(startDate, endDate);

        var response = await client.GetAsync(path, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var projection = await response.Content.ReadFromJsonAsync<ProjectionResponse>(TestContext.Current.CancellationToken);

        projection.ShouldNotBeNull();

        return projection!;
    }
}
