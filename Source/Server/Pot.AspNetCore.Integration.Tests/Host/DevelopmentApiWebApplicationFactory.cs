using Microsoft.AspNetCore.Hosting;

namespace Pot.AspNetCore.Integration.Tests.Host;

/// <summary>
/// Development-configuration factory for integration tests that require Development-only endpoints.
/// </summary>
public sealed class DevelopmentApiWebApplicationFactory : ApiWebApplicationFactory
{
    /// <summary>
    /// Creates a development-configured factory with explicit database connection details.
    /// </summary>
    /// <param name="dbHost">Database hostname from the test container.</param>
    /// <param name="dbPort">Database port from the test container.</param>
    public DevelopmentApiWebApplicationFactory(string dbHost, int dbPort)
        : base(dbHost, dbPort)
    {
    }

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.UseEnvironment("Development");
    }
}
