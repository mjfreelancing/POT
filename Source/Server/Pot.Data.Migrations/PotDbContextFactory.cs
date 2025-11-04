using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Pot.Data.Extensions;

namespace Pot.Data.Migrations;

// Only used for design time migrations
public class PotDbContextFactory : IDesignTimeDbContextFactory<PotDbContext>
{
    public PotDbContext CreateDbContext(string[] args)
    {
        var configuration = CreateConfiguration();
        var connectionString = GetConnectionString(configuration);
        var optionsBuilder = new DbContextOptionsBuilder<PotDbContext>();

        optionsBuilder.ConfigurePostgres(connectionString);

        return new PotDbContext(optionsBuilder.Options, new NullCurrentUserContext());
    }

    private static IConfiguration CreateConfiguration()
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

    // The rest of the app uses the DatabaseConfiguration class to build the connection string
    // but this requires adding DI to the factory which is overkill for this purpose.
    private static string GetConnectionString(IConfiguration configuration)
    {
        var databaseName = GetConfigurationValue(configuration, "Database:Name");
        var databaseHost = GetConfigurationValue(configuration, "Database:Host");
        var databaseUsername = GetConfigurationValue(configuration, "Database:Username");
        var databasePassword = GetConfigurationValue(configuration, "Database:Password");
        var databasePort = GetConfigurationValue(configuration, "Database:Port");
        var databaseSslMode = GetConfigurationValue(configuration, "Database:Password");

        return $"Host={databaseHost};Database={databaseName};Username={databaseUsername};Password={databasePassword};Port={databasePort};SSLMode={databaseSslMode}";
    }

    private static string GetConfigurationValue(IConfiguration configuration, string key)
    {
        // Not using: configuration.GetValue<string>(key)
        // since it would require adding Microsoft.Extensions.Configuration.Binder as a dependency
        return configuration[key] ?? throw new InvalidOperationException($"Configuration value for '{key}' is not set.");
    }
}
