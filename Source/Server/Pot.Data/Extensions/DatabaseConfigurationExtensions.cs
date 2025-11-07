using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pot.Data.Configuration;
using Pot.Shared.Extensions;

namespace Pot.Data.Extensions;

public static class DatabaseConfigurationExtensions
{
    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .ConfigureOptions<DatabaseConfigurationSetup>()

            // Allow for injection of DatabaseConfiguration instead of IOptions<DatabaseConfiguration>
            .AddSingletonFromOptions<DatabaseConfiguration>();

        return services;
    }

    public static string GetConnectionString(this DatabaseConfiguration configuration)
    {
        return $"Host={configuration.Host};Database={configuration.Name};Username={configuration.Username};Password={configuration.Password};Port={configuration.Port};SSLMode={configuration.SSLMode}";
    }
}
