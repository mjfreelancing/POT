using Microsoft.AspNetCore.Hosting;

namespace Pot.AspNetCore.Integration.Tests.Host;

public sealed class ProductionApiWebApplicationFactory : ApiWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.UseEnvironment("Production");
    }
}