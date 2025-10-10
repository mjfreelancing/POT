using Pot.AspNetCore.Features.DbBackup.Configuration;
using Pot.AspNetCore.Features.DbBackup.Workers;
using Pot.Shared.Extensions;

namespace Pot.AspNetCore.Features.DbBackup.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDbBackup(this IServiceCollection services, IHostEnvironment environment, IConfiguration configuration)
    {
        services
            .AddHostedService<DbBackupWorker>()
            .AddHostedService<DbCleanupWorker>();

        // Used by PostgresqlBackup which is auto-registered
        services
            .ConfigureOptions<BackupConfigurationSetup>()

            // Allow for injection of BackupConfiguration instead of IOptions<BackupConfiguration>
            .AddSingletonFromOptions<BackupConfiguration>();

        return services;
    }
}

