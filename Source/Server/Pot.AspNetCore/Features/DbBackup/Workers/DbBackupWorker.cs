using AllOverIt.Assertion;
using AllOverIt.GenericHost;
using Cronos;
using Microsoft.Extensions.Options;
using Pot.App.Concerns.Time;
using Pot.App.Concerns.Time.Extensions;
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
                    await postgresqlBackup.ExecuteAsync(stoppingToken).ConfigureAwait(false);

                    var currentUtc = _timeProvider.GetUtcDateTimeNow();

                    // Should never fail since the cron expression has already been validated
                    nextUtc = cronExpression.GetNextOccurrence(currentUtc);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (OptionsValidationException exception)
                {
                    // PostgresqlBackup depends on BackupConfiguration, which is validated on startup via IValidateOptions on BackupConfigurationSetup
                    logger.LogError(exception, "The database backup configuration is invalid: {ExceptionMessage}", exception.Message);

                    // TODO: Report this somewhere more visible than just the logs - ? admin user(s)?

                    return; // No point continuing if the configuration is invalid since it won't change until the app restarts
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "An error occurred during the database backup process: {ExceptionMessage}", exception.Message);
                }
            }

            if (nextUtc.HasValue)
            {
                logger.LogInformation("Next database backup scheduled for {NextBackupTimeUtc:O} (UTC)", nextUtc);

                await _timeProvider.WaitUntilUtcAsync(nextUtc.Value, stoppingToken);
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
        // The database backup is for all sites, so this is NOT site specific - hence based on UTC
        if (!CronExpression.TryParse(backupSettings.Schedule, out var cronExpression))
        {
            logger.LogError("Database backup schedule is not a valid cron expression: {BackupSchedule}", backupSettings.Schedule);
            return false;
        }

        return true;
    }
}
