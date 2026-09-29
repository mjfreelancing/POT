using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Pot.AspNetCore.Integration.Tests.Host.Models;
using Pot.Shared.Enumerations;
using Shouldly;
using System.Net;

namespace Pot.AspNetCore.Integration.Tests.Features.Incomes;

public class IncomeRenewAndExcludeFixture : IntegrationAuthFixtureBase
{
    private const double AccountBalance = 1000.0d;
    private const double IncomeAmount = 250.0d;
    private const int CycleDays = 30;

    // Incomes carry no accrual, so a renewal only has to move the schedule on.
    [Fact]
    public async Task Should_Catch_An_Overdue_Income_Up_To_Today()
    {
        var admin = await CreateAdminUserAsync("incomes", "Incomes User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();
        var incomeRowId = await CreateIncomeAsync(client, today, today.AddDays(-45));

        var response = await client.RenewIncomesAsync([incomeRowId], today, RenewalMode.Overdue);

        await response.ShouldHaveStatusAsync(HttpStatusCode.OK);

        var renewed = await client.GetIncomeAsync(incomeRowId);

        renewed.NextDue.ShouldBe(today.AddDays(15), "two whole cycles are caught up, so the next occurrence is half a cycle ahead");
    }

    [Fact]
    public async Task Should_Advance_A_Future_Income_By_Exactly_One_Cycle()
    {
        var admin = await CreateAdminUserAsync("incomes", "Incomes User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();
        var incomeRowId = await CreateIncomeAsync(client, today, today.AddDays(5));

        var response = await client.RenewIncomesAsync([incomeRowId], today, RenewalMode.Future);

        await response.ShouldHaveStatusAsync(HttpStatusCode.OK);

        var renewed = await client.GetIncomeAsync(incomeRowId);

        renewed.NextDue.ShouldBe(today.AddDays(35), "the future mode moves on by one cycle");
    }

    [Fact]
    public async Task Should_Return_UnprocessableEntity_When_Renewing_With_No_Incomes_Selected()
    {
        var admin = await CreateAdminUserAsync("incomes", "Incomes User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var response = await client.RenewIncomesAsync([], GetSiteLocalDateToday(), RenewalMode.Overdue);

        await response.ShouldHaveStatusAsync(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Should_Toggle_Exclude_On_An_Income()
    {
        var admin = await CreateAdminUserAsync("incomes", "Incomes User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();
        var incomeRowId = await CreateIncomeAsync(client, today, today.AddDays(CycleDays));

        var response = await client.ToggleExcludeIncomesAsync([incomeRowId]);

        await response.ShouldHaveStatusAsync(HttpStatusCode.OK);

        var toggled = await client.GetIncomeAsync(incomeRowId);

        toggled.ExcludeFromCalcs.ShouldBeTrue("the toggle flips the flag it was created with");

        var secondResponse = await client.ToggleExcludeIncomesAsync([incomeRowId]);

        await secondResponse.ShouldHaveStatusAsync(HttpStatusCode.OK);

        var toggledBack = await client.GetIncomeAsync(incomeRowId);

        toggledBack.ExcludeFromCalcs.ShouldBeFalse("a second toggle flips it back");
    }

    [Fact]
    public async Task Should_Return_UnprocessableEntity_When_Toggling_With_No_Incomes_Selected()
    {
        var admin = await CreateAdminUserAsync("incomes", "Incomes User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var response = await client.ToggleExcludeIncomesAsync([]);

        await response.ShouldHaveStatusAsync(HttpStatusCode.UnprocessableEntity);
    }

    private static async Task<Guid> CreateIncomeAsync(HttpClient client, DateOnly today, DateOnly nextDue)
    {
        var accountRowId = await client.CreateAccountAsync($"Income Account {Guid.NewGuid():N}", AccountBalance);
        var request = IncomeRequestFactory.CreateIncome(accountRowId, nextDue, Frequency.Days, CycleDays, IncomeAmount);

        return await client.CreateIncomeAsync(request);
    }
}
