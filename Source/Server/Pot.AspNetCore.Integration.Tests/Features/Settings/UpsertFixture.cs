using Pot.AspNetCore.Integration.Tests.Host;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Pot.AspNetCore.Integration.Tests.Features.Settings;

/// <summary>
/// Proves a settings validation failure never returns the value the caller put in the route.
/// </summary>
public class UpsertFixture : IntegrationAuthFixtureBase
{
    [Fact]
    public async Task Should_Not_Echo_The_Submitted_Category_In_A_Validation_Message()
    {
        var user = await CreateAdminUserAsync("settings-category", "Settings Category");
        using var client = await CreateAuthenticatedClientAsync(user);

        const string category = "not-a-category";
        var payload = new
        {
            Value = "1"
        };

        using var response = await client.PutAsJsonAsync($"/api/settings/{category}/anything", payload,
            TestContext.Current.CancellationToken);

        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // 'instance' repeats the request path, which the caller already supplied, so only the message
        // fields are checked.
        using var document = JsonDocument.Parse(body);
        var problem = document.RootElement;

        var detail = problem.GetProperty("detail").GetString() ?? string.Empty;

        var error = problem.GetProperty("errors").EnumerateArray().ShouldHaveSingleItem();

        var propertyName = error.GetProperty("propertyName").GetString() ?? string.Empty;
        var errorMessage = error.GetProperty("errorMessage").GetString() ?? string.Empty;

        detail.ShouldNotContain(category);
        propertyName.ShouldNotContain(category);
        errorMessage.ShouldNotContain(category);
    }
}
