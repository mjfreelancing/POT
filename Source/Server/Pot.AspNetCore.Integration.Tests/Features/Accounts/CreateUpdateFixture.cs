using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Shouldly;
using System.Net.Http.Json;

namespace Pot.AspNetCore.Integration.Tests.Features.Accounts;

public class CreateUpdateFixture : IntegrationAuthFixtureBase
{
    private const double AccountBalance = 1000.0d;
    private const string DuplicateDescriptionMessage = "a duplicate account description must not be accepted";
    private const string DuplicateDescriptionError = "The account description already exists";

    [Fact]
    public async Task Should_Return_Conflict_When_Creating_An_Account_With_An_Existing_Description()
    {
        var admin = await CreateAdminUserAsync("accounts", "Accounts User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var description = $"Account {Guid.NewGuid():N}";

        await client.CreateAccountAsync(description, AccountBalance);

        var duplicate = new
        {
            Description = description,
            Balance = AccountBalance,
            Reserved = 0.0d
        };

        var response = await client.PostAsJsonAsync("/api/accounts", duplicate, TestContext.Current.CancellationToken);

        var problem = await response.ShouldBeEntityConflictAsync(DuplicateDescriptionMessage);

        problem.Detail.ShouldBe(DuplicateDescriptionError);
    }

    [Fact]
    public async Task Should_Return_Conflict_When_Updating_An_Account_To_An_Existing_Description()
    {
        var admin = await CreateAdminUserAsync("accounts", "Accounts User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var existingDescription = $"Account {Guid.NewGuid():N}";

        await client.CreateAccountAsync(existingDescription, AccountBalance);

        var accountToUpdate = await client.CreateAccountAsync($"Account {Guid.NewGuid():N}", AccountBalance);
        var current = await client.GetAccountAsync(accountToUpdate);

        var request = new
        {
            Etag = current.Etag,
            Description = existingDescription,
            Balance = current.Balance,
            Reserved = current.Reserved
        };

        var response = await client.PutAsJsonAsync($"/api/accounts/{accountToUpdate}", request,
            TestContext.Current.CancellationToken);

        var problem = await response.ShouldBeEntityConflictAsync(DuplicateDescriptionMessage);

        problem.Detail.ShouldBe(DuplicateDescriptionError);
    }
}
