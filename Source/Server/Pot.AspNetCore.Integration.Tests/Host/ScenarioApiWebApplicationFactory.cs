using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Pot.AspNetCore.Integration.Tests.Host;

/// <summary>
/// Production-configuration factory for fixtures that provision several callers and drive many operations from
/// each of them.
/// </summary>
/// <remarks>
/// The host enforces the production rate limits by default, and every client a test creates shares one anonymous
/// partition because they all arrive from the same address. A fixture that provisions tenants through the routes
/// spends several anonymous requests before its first assertion, so it would be throttled for doing the setup rather
/// than for the behaviour it is testing. The limits are raised here, for this factory alone: every other fixture
/// keeps the production values, and <c>HeadersAndRateLimitFixture</c> continues to prove them.
/// </remarks>
public sealed class ScenarioApiWebApplicationFactory : ApiWebApplicationFactory
{
    private const int UnthrottledPermitLimit = 5_000;

    /// <summary>
    /// Creates the factory with explicit database connection details.
    /// </summary>
    /// <param name="dbHost">Database hostname from the TestContainers container.</param>
    /// <param name="dbPort">Database port from the TestContainers container's mapped 5432 port.</param>
    public ScenarioApiWebApplicationFactory(string dbHost, int dbPort)
        : base(dbHost, dbPort)
    {
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.UseEnvironment("Production");

        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RateLimiting:Anonymous:PermitLimit"] = UnthrottledPermitLimit.ToString(),
                ["RateLimiting:Authenticated:PermitLimit"] = UnthrottledPermitLimit.ToString()
            });
        });
    }
}
