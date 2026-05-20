using Pot.AspNetCore.Integration.Tests.Host;
using Shouldly;
using System.Net;

namespace Pot.AspNetCore.Integration.Tests.Features.Health;

public class HealthCheckFixture : IntegrationFixtureBase
{
    [Fact]
    public async Task Should_Return_Ok_When_Database_Context_Is_Available()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/_health/ready");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}

public class HealthCheckUnhealthyFixture : IAsyncLifetime
{
    // Port 59999 on loopback is intentionally unreachable — no service binds there.
    // A TCP connection attempt gets an immediate "connection refused", so the EF Core
    // health check fails fast without waiting for a timeout.
    private const int UnreachableDbPort = 59999;

    private ProductionApiWebApplicationFactory? _factory;

    async Task IAsyncLifetime.InitializeAsync()
    {
        _factory = new ProductionApiWebApplicationFactory("127.0.0.1", UnreachableDbPort);

        await Task.CompletedTask;
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        _factory?.Dispose();
        _factory = null;

        await Task.CompletedTask;
    }

    [Fact]
    public async Task Should_Return_ServiceUnavailable_When_Database_Is_Not_Available()
    {
        _factory.ShouldNotBeNull();

        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/_health/ready");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
