using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Pot.AspNetCore.Integration.Tests.Host.Models;
using Pot.Shared.Enumerations;
using Shouldly;
using System.Net;

namespace Pot.AspNetCore.Integration.Tests.Features.Incomes;

public class ToggleExcludeFixture : IntegrationAuthFixtureBase
{
    private const double AccountBalance = 1000.0d;
    private const double IncomeAmount = 250.0d;
    private const int CycleDays = 30;

    [Fact]
    public async Task Should_Toggle_Exclude_On_An_Income()
    {
        var admin = await CreateAdminUserAsync("incomes", "Incomes User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();
        var accountRowId = await client.CreateAccountAsync($"Income Account {Guid.NewGuid():N}", AccountBalance);
        var request = IncomeRequestFactory.CreateIncome(accountRowId, today.AddDays(CycleDays), Frequency.Days,
            CycleDays, IncomeAmount);
        var incomeRowId = await client.CreateIncomeAsync(request);

        var response = await client.ToggleExcludeIncomesAsync([incomeRowId]);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var toggled = await client.GetIncomeAsync(incomeRowId);

        toggled.ExcludeFromCalcs.ShouldBeTrue("the toggle flips the flag it was created with");

        var secondResponse = await client.ToggleExcludeIncomesAsync([incomeRowId]);

        secondResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var toggledBack = await client.GetIncomeAsync(incomeRowId);

        toggledBack.ExcludeFromCalcs.ShouldBeFalse("a second toggle flips it back");
    }

    [Fact]
    public async Task Should_Return_BadRequest_When_Toggling_With_No_Incomes_Selected()
    {
        var admin = await CreateAdminUserAsync("incomes", "Incomes User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var response = await client.ToggleExcludeIncomesAsync([]);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
