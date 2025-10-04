using AllOverIt.Assertion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Pot.Data.Extensions;

public static class DatabaseConfigurationExtensions
{
    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .Configure<DatabaseConfiguration>(options => configuration.GetSection("DATABASE")
            .Bind(options));

        // Validate required fields
        services.PostConfigure<DatabaseConfiguration>(options =>
        {
            _ = options.Host.WhenNotNullOrEmpty();
            _ = options.Name.WhenNotNullOrEmpty();
            _ = options.Username.WhenNotNullOrEmpty();
            _ = options.Password.WhenNotNullOrEmpty();
        });

        return services;
    }

    public static string GetConnectionString(this DatabaseConfiguration configuration)
    {
        return $"Host={configuration.Host};Database={configuration.Name};Username={configuration.Username};Password={configuration.Password}";
    }
}
