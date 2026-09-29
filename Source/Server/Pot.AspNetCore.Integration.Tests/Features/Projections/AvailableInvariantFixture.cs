using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Pot.AspNetCore.Integration.Tests.Host.Models;
using Pot.Shared.Enumerations;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace Pot.AspNetCore.Integration.Tests.Features.Projections;

public class AvailableInvariantFixture : IntegrationAuthFixtureBase
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
        public double Available { get; set; }
    }

    // Today is a payment day, so the forecast has already paid the bill. Available has to be measured before the
    // schedule advances, otherwise it credits the day for a payment that was never accrued and reads above the
    // balance the same forecast produced.
    [Fact]
    public async Task Should_Not_Report_Available_Above_Balance_On_A_Payment_Day()
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
        paymentDay.Available.ShouldBe(AccountBalance - BillAmount);

        AssertAvailableNeverExceedsBalance(account.Dates);
        AssertAvailableNeverExceedsBalance(projection.Global);
    }

    // A bill that does not accrue is still paid on its due date, so the day must not be credited for an accrual
    // it never made. The add-back exists to cancel a double count, not to hand back money.
    [Fact]
    public async Task Should_Not_Count_A_Non_Accruing_Payment_Day_As_Available()
    {
        var admin = await CreateAdminUserAsync("projections", "Projections User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();

        await SeedPaymentDayAsync(client, today, AccrualPolicy.None);

        var projection = await GetProjectionAsync(client, today, today.AddDays(WindowDays - 1));
        var paymentDay = projection.Accounts.ShouldHaveSingleItem().Dates[0];

        paymentDay.Balance.ShouldBe(AccountBalance - BillAmount);
        paymentDay.Available.ShouldBe(AccountBalance - BillAmount);
        paymentDay.Available.ShouldBeLessThanOrEqualTo(paymentDay.Balance);
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
        paymentDay.Available.ShouldBe(-BillAmount);
    }

    private static void AssertAvailableNeverExceedsBalance(IEnumerable<DateProjection> dates)
    {
        foreach (var date in dates)
        {
            date.Available.ShouldBeLessThanOrEqualTo(date.Balance, "Available must never exceed Balance");
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

    private static async Task<ProjectionResponse> GetProjectionAsync(HttpClient client, DateOnly startDate, DateOnly endDate)
    {
        var path = $"{ProjectionsPath}?StartDate={startDate:yyyy-MM-dd}&EndDate={endDate:yyyy-MM-dd}";

        var response = await client.GetAsync(path, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var projection = await response.Content.ReadFromJsonAsync<ProjectionResponse>(TestContext.Current.CancellationToken);

        projection.ShouldNotBeNull();

        return projection!;
    }
}
