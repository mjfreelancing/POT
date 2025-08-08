using Microsoft.Extensions.Configuration;

namespace Pot.Data.Migrations;

internal static class ConfigurationFactory
{
    public static IConfiguration Create()
    {
        return new ConfigurationBuilder()

            // Using this instead of launchsettings.json during during development because the
            // add-migration and remove-migration CLI commands won't have environment variables.
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)

            // This will override launchsettings.json when running outside of development
            .AddEnvironmentVariables()

            .Build();
    }
}