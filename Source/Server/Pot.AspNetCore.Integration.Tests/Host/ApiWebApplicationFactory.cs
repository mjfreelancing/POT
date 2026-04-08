using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Pot.AspNetCore.Integration.Tests.Host;

public abstract class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins"] = "http://localhost:3000,https://www.localhost:3000",
                ["Database:Name"] = "pot_test",
                ["Database:Host"] = "localhost",
                ["Database:Username"] = "test",
                ["Database:Password"] = "test",
                ["Database:Port"] = "5432",
                ["Database:SSLMode"] = "Disable",
                ["Jwt:Issuer"] = "pot-integration-tests",
                ["Jwt:Audience"] = "pot-integration-tests",
                ["Jwt:SecretKey"] = "pot-integration-tests-secret-key-32",
                ["Smtp:Host"] = "localhost",
                ["Smtp:Port"] = "2525",
                ["Smtp:RequireTls"] = "false",
                ["Smtp:Authentication:Username"] = "test",
                ["Smtp:Authentication:Password"] = "test",
                ["Smtp:From:Name"] = "POT Tests",
                ["Smtp:From:Address"] = "pot-tests@example.com",
                ["Authentication:Cookie:Name"] = "pot-auth",
                ["Authentication:Cookie:Path"] = "/",
                ["Authentication:Cookie:SameSite"] = "Lax",
                ["Authentication:Cookie:SecurePolicy"] = "None",
                ["PlatformAdmin:UserIds"] = string.Empty
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IHostedService>();

            services.AddFakeLogging(options =>
            {
                options.CollectRecordsForDisabledLogLevels = true;
            });
        });
    }
}