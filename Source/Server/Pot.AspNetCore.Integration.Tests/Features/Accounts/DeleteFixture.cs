using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Shouldly;
using System.Net;

namespace Pot.AspNetCore.Integration.Tests.Features.Accounts;

public class DeleteFixture : IntegrationAuthFixtureBase
{
    [Fact]
    public async Task Should_Return_NoContent_When_Deleting_An_Account()
    {
        var admin = await CreateAdminUserAsync("accounts", "Accounts User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var accountRowId = await client.CreateAccountAsync($"Delete Account {Guid.NewGuid():N}", 1000.0d);
        var response = await client.DeleteAsync($"/api/accounts/{accountRowId}", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        response.Content.Headers.ContentLength.ShouldBe(0);
    }
}
