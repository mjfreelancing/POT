using AllOverIt.Assertion;
using AllOverIt.GenericHost;
using Cronos;
using Pot.App.Concerns.Time;

namespace Pot.AspNetCore.Features.DbBackup;

internal sealed class DbBackupWorker : BackgroundWorker
{
    private readonly ITimeProvider _timeProvider;
    private readonly IServiceScopeFactory _scopeFactory;

    public DbBackupWorker(IHostApplicationLifetime applicationLifetime, ITimeProvider timeProvider,
        IServiceScopeFactory scopeFactory)
        : base(applicationLifetime)
    {
        _timeProvider = timeProvider.WhenNotNull();
        _scopeFactory = scopeFactory.WhenNotNull();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // The database backup is for all sites, so this is NOT site specific - hence based on UTC
        var cronExpression = CronExpression.Parse("15 * * * *"); // 15 minutes past the hour, every hour

        while (!stoppingToken.IsCancellationRequested)
        {
            var currentUtc = _timeProvider.GetUtcDateTimeNow();
            var nextUtc = cronExpression.GetNextOccurrence(currentUtc);

            if (!nextUtc.HasValue)
            {
                LogError($"Failed to determine the next Db Backup occurrence from the cron expression {cronExpression}");
                break;
            }

            var delayTimespan = nextUtc.Value - currentUtc;

            if (delayTimespan.TotalMilliseconds > 0)
            {
                await Task.Delay(delayTimespan, stoppingToken).ConfigureAwait(false);
            }

            // Limit the lifetime of the scope to this backup iteration
            using var scope = _scopeFactory.CreateScope();

            var logger = scope.ServiceProvider.GetRequiredService<ILogger<DbBackupWorker>>();

            try
            {
                var postgresqlBackup = scope.ServiceProvider.GetRequiredService<IPostgresqlBackup>();
                await postgresqlBackup.ExecuteAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "An error occurred during the database backup process: {ExceptionMessage}", exception.Message);
            }
        }
    }

    private void LogError(string message)
    {
        using var scope = _scopeFactory.CreateScope();

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DbBackupWorker>>();
        logger.LogError("{ErrorMesage}", message);
    }
}
