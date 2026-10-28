using Shouldly;
using System.Net;

namespace Pot.AspNetCore.Integration.Tests.Features.Health;

public class HealthCheckFixture : IntegrationFixtureBase
{
    [Fact]
    public async Task Should_Return_Ok_When_Database_Context_Is_Available()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/_health/ready", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
