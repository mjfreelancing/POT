using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Pot.AspNetCore.Integration.Tests.Host.Models;
using Pot.Shared.Enumerations;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace Pot.AspNetCore.Integration.Tests.Features.Incomes;

public class IncomeCrudFixture : IntegrationAuthFixtureBase
{
    private const double AccountBalance = 1000.0d;
    private const double IncomeAmount = 250.0d;
    private const int CycleDays = 30;

    private const string StaleEtagMessage = "a stale entity tag must not be accepted";

    private sealed class ValidationProblem
    {
        public ValidationError[] Errors { get; set; } = [];
    }

    private sealed class ValidationError
    {
        public string? PropertyName { get; set; }
    }

    [Fact]
    public async Task Should_Return_Unauthorized_When_Creating_Without_Authentication()
    {
        using var client = CreateClient();

        var request = IncomeRequestFactory.CreateIncome(Guid.NewGuid(), GetSiteLocalDateToday(), Frequency.Days,
            CycleDays, IncomeAmount);

        var response = await client.PostAsJsonAsync("/api/incomes", request, TestContext.Current.CancellationToken);

        await response.ShouldHaveStatusAsync(HttpStatusCode.Unauthorized);
    }

    // Both read endpoints are separate response types, so both are asserted.
    [Fact]
    public async Task Should_Create_An_Income_And_Return_It_From_Both_Read_Endpoints()
    {
        var admin = await CreateAdminUserAsync("incomes", "Incomes User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();
        var accountRowId = await client.CreateAccountAsync($"Income Account {Guid.NewGuid():N}", AccountBalance);
        var request = IncomeRequestFactory.CreateIncome(accountRowId, today.AddDays(CycleDays), Frequency.Days,
            CycleDays, IncomeAmount);

        var incomeRowId = await client.CreateIncomeAsync(request);

        var listed = (await client.GetIncomesAsync()).ShouldHaveSingleItem();

        AssertCreatedIncome(listed, incomeRowId, request, accountRowId);

        var fetched = await client.GetIncomeAsync(incomeRowId);

        AssertCreatedIncome(fetched, incomeRowId, request, accountRowId);
    }

    [Fact]
    public async Task Should_Return_UnprocessableEntity_When_The_Description_Is_Blank()
    {
        var admin = await CreateAdminUserAsync("incomes", "Incomes User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();
        var accountRowId = await client.CreateAccountAsync($"Income Account {Guid.NewGuid():N}", AccountBalance);
        var request = IncomeRequestFactory.CreateIncome(accountRowId, today.AddDays(CycleDays), Frequency.Days,
            CycleDays, IncomeAmount) with
        { Description = string.Empty };

        var response = await client.PostAsJsonAsync("/api/incomes", request, TestContext.Current.CancellationToken);

        await response.ShouldHaveStatusAsync(HttpStatusCode.UnprocessableEntity);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>(TestContext.Current.CancellationToken);

        problem.ShouldNotBeNull();
        problem!.Errors.Select(error => error.PropertyName).ShouldContain("Description");
    }

    [Fact]
    public async Task Should_Update_An_Income()
    {
        const double editedAmount = IncomeAmount * 2;

        var admin = await CreateAdminUserAsync("incomes", "Incomes User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();
        var accountRowId = await client.CreateAccountAsync($"Income Account {Guid.NewGuid():N}", AccountBalance);
        var request = IncomeRequestFactory.CreateIncome(accountRowId, today.AddDays(CycleDays), Frequency.Days,
            CycleDays, IncomeAmount);

        var incomeRowId = await client.CreateIncomeAsync(request);
        var created = await client.GetIncomeAsync(incomeRowId);
        var updateRequest = request with { Etag = created.Etag, Amount = editedAmount };

        await client.UpdateIncomeAsync(incomeRowId, updateRequest);

        var updated = await client.GetIncomeAsync(incomeRowId);

        updated.Amount.ShouldBe(editedAmount);
        updated.Description.ShouldBe(request.Description, "an update keeps the values it did not change");
    }

    // A stale tag is a concurrency conflict rather than a validation failure, and the status says so.
    [Fact]
    public async Task Should_Return_Conflict_When_The_Etag_Is_Stale()
    {
        var admin = await CreateAdminUserAsync("incomes", "Incomes User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();
        var accountRowId = await client.CreateAccountAsync($"Income Account {Guid.NewGuid():N}", AccountBalance);
        var request = IncomeRequestFactory.CreateIncome(accountRowId, today.AddDays(CycleDays), Frequency.Days,
            CycleDays, IncomeAmount);

        var incomeRowId = await client.CreateIncomeAsync(request);
        var created = await client.GetIncomeAsync(incomeRowId);
        var staleRequest = request with { Etag = created.Etag - 1 };

        var response = await client.PutAsJsonAsync($"/api/incomes/{incomeRowId}", staleRequest, TestContext.Current.CancellationToken);

        await response.ShouldHaveStatusAsync(HttpStatusCode.Conflict, StaleEtagMessage);
    }

    [Fact]
    public async Task Should_Return_NotFound_When_Updating_An_Income_That_Does_Not_Exist()
    {
        var admin = await CreateAdminUserAsync("incomes", "Incomes User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();
        var accountRowId = await client.CreateAccountAsync($"Income Account {Guid.NewGuid():N}", AccountBalance);
        var request = IncomeRequestFactory.CreateIncome(accountRowId, today.AddDays(CycleDays), Frequency.Days,
            CycleDays, IncomeAmount);

        var response = await client.PutAsJsonAsync($"/api/incomes/{Guid.NewGuid()}", request, TestContext.Current.CancellationToken);

        await response.ShouldHaveStatusAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_Delete_An_Income()
    {
        var admin = await CreateAdminUserAsync("incomes", "Incomes User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var today = GetSiteLocalDateToday();
        var accountRowId = await client.CreateAccountAsync($"Income Account {Guid.NewGuid():N}", AccountBalance);
        var request = IncomeRequestFactory.CreateIncome(accountRowId, today.AddDays(CycleDays), Frequency.Days,
            CycleDays, IncomeAmount);

        var incomeRowId = await client.CreateIncomeAsync(request);

        var response = await client.DeleteIncomeAsync(incomeRowId);

        await response.ShouldHaveStatusAsync(HttpStatusCode.OK);
        (await client.GetIncomesAsync()).ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Return_NotFound_When_Deleting_An_Income_That_Does_Not_Exist()
    {
        var admin = await CreateAdminUserAsync("incomes", "Incomes User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var response = await client.DeleteIncomeAsync(Guid.NewGuid());

        await response.ShouldHaveStatusAsync(HttpStatusCode.NotFound);
    }

    private static void AssertCreatedIncome(IncomeResponse income, Guid incomeRowId, IncomeRequest request, Guid accountRowId)
    {
        income.RowId.ShouldBe(incomeRowId);
        income.Description.ShouldBe(request.Description);
        income.Amount.ShouldBe(IncomeAmount);
        income.NextDue.ShouldBe(DateOnly.ParseExact(request.NextDue, "yyyy-MM-dd"));
        income.ExcludeFromCalcs.ShouldBeFalse("a new income is included in calculations");
        income.Account.ShouldNotBeNull();
        income.Account!.RowId.ShouldBe(accountRowId);
    }
}
