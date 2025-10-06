using Pot.AspNetCore.Features.DbBackup.Workers;

namespace Pot.AspNetCore.Features.DbBackup.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDbBackupWorker(this IServiceCollection services)
    {
        services
            .AddHostedService<DbBackupWorker>()
            .AddHostedService<DbCleanupWorker>();

        return services;
    }
}