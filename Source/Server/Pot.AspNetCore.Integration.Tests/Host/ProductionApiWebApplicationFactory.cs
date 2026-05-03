using Microsoft.AspNetCore.Hosting;

namespace Pot.AspNetCore.Integration.Tests.Host;

/// <summary>
/// Concrete production-configuration factory for integration tests.
/// Accepts database connection details from the calling fixture's container.
/// See <see cref="ApiWebApplicationFactory"/> for the initialization pattern explanation.
/// </summary>
public sealed class ProductionApiWebApplicationFactory : ApiWebApplicationFactory
{
    /// <summary>
    /// Creates a production-configured factory with explicit database connection details.
    /// </summary>
    /// <param name="dbHost">Database hostname from TestContainers container</param>
    /// <param name="dbPort">Database port from TestContainers container GetMappedPublicPort(5432)</param>
    public ProductionApiWebApplicationFactory(string dbHost, int dbPort)
        : base(dbHost, dbPort)
    {
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.UseEnvironment("Production");
    }
}