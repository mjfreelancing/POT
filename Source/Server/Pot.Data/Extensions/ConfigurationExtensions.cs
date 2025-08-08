using Microsoft.Extensions.Configuration;

namespace Pot.Data.Extensions;

public static class ConfigurationExtensions
{
    public static string GetConnectionString(this IConfiguration configuration)
    {
        var databaseHost = GetConfigurationValue(configuration, "Database:Host");
        var databaseUsername = GetConfigurationValue(configuration, "Database:Username");
        var databasePassword = GetConfigurationValue(configuration, "Database:Password");

        return $"Host={databaseHost};Database=Pot;Username={databaseUsername};Password={databasePassword}";
    }

    private static string GetConfigurationValue(IConfiguration configuration, string key)
    {
        // Not using: configuration.GetValue<string>(key)
        // since it would require adding Microsoft.Extensions.Configuration.Binder as a dependency
        return configuration[key] ?? throw new InvalidOperationException($"Configuration value for '{key}' is not set.");
    }
}