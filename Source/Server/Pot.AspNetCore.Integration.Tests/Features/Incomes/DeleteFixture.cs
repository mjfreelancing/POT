using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Pot.AspNetCore.Integration.Tests.Host.Models;
using Pot.Shared.Enumerations;
using Shouldly;
using System.Net;

namespace Pot.AspNetCore.Integration.Tests.Features.Incomes;

public class DeleteFixture : IntegrationAuthFixtureBase
{
    private const double AccountBalance = 1000.0d;
    private const double IncomeAmount = 250.0d;
    private const int CycleDays = 30;

    [Fact]
    public async Task Should_Return_NoContent_When_Deleting_An_Income()
    {
        var admin = await CreateAdminUserAsync("incomes", "Incomes User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();
        var accountRowId = await client.CreateAccountAsync($"Income Account {Guid.NewGuid():N}", AccountBalance);
        var request = IncomeRequestFactory.CreateIncome(accountRowId, today.AddDays(CycleDays), Frequency.Days,
            CycleDays, IncomeAmount);
        var incomeRowId = await client.CreateIncomeAsync(request);
        var response = await client.DeleteIncomeAsync(incomeRowId);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        response.Content.Headers.ContentLength.ShouldBe(0);
        (await client.GetIncomesAsync()).ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Return_NotFound_When_Deleting_An_Income_That_Does_Not_Exist()
    {
        var admin = await CreateAdminUserAsync("incomes", "Incomes User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var response = await client.DeleteIncomeAsync(Guid.NewGuid());

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
