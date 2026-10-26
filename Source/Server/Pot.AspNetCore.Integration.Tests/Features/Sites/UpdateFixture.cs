using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Shouldly;
using System.Net.Http.Json;

namespace Pot.AspNetCore.Integration.Tests.Features.Sites;

/// <summary>
/// Proves an update that carries an entity tag the store no longer holds is refused as a stale conflict.
/// </summary>
public class UpdateFixture : IntegrationAuthFixtureBase
{
    private const string StaleEtagMessage = "a stale entity tag must not be accepted";
    private const string StaleEtagError = "The entity tag for the Site does not match the current record";

    [Fact]
    public async Task Should_Return_Conflict_When_The_Etag_Is_Stale()
    {
        var admin = await CreateAdminUserAsync("site-update", "Site Update User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var currentUser = await client.GetCurrentUserAsync();
        var site = currentUser.Site;

        var request = new
        {
            Etag = site.Etag - 1,
            Name = site.Name,
            Description = site.Description
        };

        using var response = await client.PutAsJsonAsync($"/api/sites/{site.RowId}", request,
            TestContext.Current.CancellationToken);

        var problem = await response.ShouldBeStaleEtagConflictAsync(StaleEtagError, StaleEtagMessage);

        problem.Detail.ShouldBe(StaleEtagError);
    }
}
