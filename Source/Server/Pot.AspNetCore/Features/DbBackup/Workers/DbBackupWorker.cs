using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.GenericHost;
using Cronos;
using Pot.App.Concerns.Time;
using Pot.Data.Repositories.Settings;
using Pot.Data.Repositories.Settings.Models;

namespace Pot.AspNetCore.Features.DbBackup.Workers;

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
        while (!stoppingToken.IsCancellationRequested)
        {
            ILogger logger;
            DateTime? nextUtc = null;

            using (var scope = _scopeFactory.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                logger = serviceProvider.GetRequiredService<ILogger<DbBackupWorker>>();

                try
                {
                    var settingsRepository = serviceProvider.GetRequiredService<ISettingsRepository>();
                    var backupSettings = await GetBackupSettingsAsync(settingsRepository, stoppingToken).ConfigureAwait(false);

                    if (!backupSettings.Enabled || !ValidBackupSettings(backupSettings, logger))
                    {
                        // Try again later in case the backup is enabled
                        nextUtc = _timeProvider.GetUtcDateTimeNow().AddMinutes(1);
                        continue;
                    }

                    // The database backup is for all sites, so this is NOT site specific - hence based on UTC
                    var cronExpression = CronExpression.Parse(backupSettings.Schedule);

                    // Perform an initial backup immediately on startup
                    var postgresqlBackup = serviceProvider.GetRequiredService<IPostgresqlBackup>();
                    await postgresqlBackup.ExecuteAsync(backupSettings.Path!, stoppingToken).ConfigureAwait(false);

                    var currentUtc = _timeProvider.GetUtcDateTimeNow();

                    // Should never fail since the cron expression has already been validated
                    nextUtc = cronExpression.GetNextOccurrence(currentUtc);
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

            if (nextUtc.HasValue)
            {
                logger.LogInformation("Next database backup scheduled for {NextBackupTimeUtc:O} (UTC)", nextUtc);

                await WaitUntilAsync(nextUtc.Value, stoppingToken);
            }
        }
    }

    private static async Task<BackupSettings> GetBackupSettingsAsync(ISettingsRepository settingsRepository, CancellationToken cancellationToken)
    {
        return await settingsRepository
            .GetDatabaseSettingsAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    private static bool ValidBackupSettings(BackupSettings backupSettings, ILogger logger)
    {
        var hasError = false;

        if (backupSettings.Path.IsNullOrEmpty())
        {
            logger.LogError("Database backup path is not configured");
            hasError = true;
        }

        // The database backup is for all sites, so this is NOT site specific - hence based on UTC
        if (!CronExpression.TryParse(backupSettings.Schedule, out var cronExpression))
        {
            logger.LogError("Database backup schedule is not a valid cron expression: {BackupSchedule}", backupSettings.Schedule);
            hasError = true;
        }

        return !hasError;
    }

    private async Task WaitUntilAsync(DateTime targetUtc, CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Checking more than once in case the system time changes or there is drift that may 
            // result in the next occurrence being a matter of seconds later rather than the expected
            // cron schedule (the latter has been observed).
            var currentUtc = _timeProvider.GetUtcDateTimeNow();

            var delayTimespan = targetUtc - currentUtc;

            if (delayTimespan <= TimeSpan.Zero)
            {
                return;
            }

            await Task.Delay(delayTimespan, cancellationToken);
        }
    }
}
