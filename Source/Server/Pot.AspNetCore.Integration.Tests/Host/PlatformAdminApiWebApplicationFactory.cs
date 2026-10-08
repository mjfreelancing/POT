using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Pot.AspNetCore.Integration.Tests.Host;

/// <summary>
/// Concrete production-configuration factory that nominates seeded users as platform administrators.
/// </summary>
/// <remarks>
/// <see cref="ApiWebApplicationFactory" /> configures an empty platform-administrator list, so a test that needs
/// <c>platform:manage</c> has to supply the ids through the host's configuration. The factory points at the
/// calling fixture's container, so users seeded by that fixture are visible to it.
/// </remarks>
public sealed class PlatformAdminApiWebApplicationFactory : ApiWebApplicationFactory
{
    private readonly Guid[] _platformAdminRowIds;

    /// <summary>
    /// Creates a production-configured factory that treats the given users as platform administrators.
    /// </summary>
    /// <param name="dbHost">Database hostname from the TestContainers container.</param>
    /// <param name="dbPort">Database port from the TestContainers container's mapped 5432 port.</param>
    /// <param name="platformAdminRowIds">The users to nominate as platform administrators.</param>
    public PlatformAdminApiWebApplicationFactory(string dbHost, int dbPort, params Guid[] platformAdminRowIds)
        : base(dbHost, dbPort)
    {
        _platformAdminRowIds = platformAdminRowIds ?? throw new ArgumentNullException(nameof(platformAdminRowIds));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.UseEnvironment("Production");

        var userIds = string.Join(',', _platformAdminRowIds);

        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PlatformAdmin:UserIds"] = userIds
            });
        });
    }
}
