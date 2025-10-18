using AllOverIt.Assertion;
using AllOverIt.GenericHost;
using Pot.App.Concerns.Time;
using Pot.AspNetCore.Features.DbBackup.Configuration;
using Pot.Data.Repositories.Settings;
using Pot.Data.Repositories.Settings.Models;

namespace Pot.AspNetCore.Features.DbBackup.Workers;

internal sealed class DbCleanupWorker : BackgroundWorker
{
    private const int DelayMinutes = 15;

    private readonly ITimeProvider _timeProvider;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly BackupConfiguration _backupOptions;        // Never changes, so doesn't need to be scoped to each backup

    public DbCleanupWorker(IHostApplicationLifetime applicationLifetime, ITimeProvider timeProvider,
        IServiceScopeFactory scopeFactory, BackupConfiguration backupOptions)
        : base(applicationLifetime)
    {
        _timeProvider = timeProvider.WhenNotNull();
        _scopeFactory = scopeFactory.WhenNotNull();
        _backupOptions = backupOptions.WhenNotNull();
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

                logger = serviceProvider.GetRequiredService<ILogger<DbCleanupWorker>>();

                try
                {
                    var settingsRepository = serviceProvider.GetRequiredService<ISettingsRepository>();
                    var backupSettings = await GetBackupSettingsAsync(settingsRepository, stoppingToken).ConfigureAwait(false);

                    if (!backupSettings.Enabled || backupSettings.RetentionDays < 1)
                    {
                        // Try again later in case the backup is enabled
                        nextUtc = _timeProvider.GetUtcDateTimeNow().AddMinutes(1);
                        continue;
                    }

                    var backupFileCleaner = serviceProvider.GetRequiredService<IBackupFileCleaner>();

                    await backupFileCleaner
                        .RemoveOldFilesAsync(_backupOptions.BackupPath, backupSettings.RetentionDays, stoppingToken)
                        .ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "An error occurred during the database cleanup process: {ExceptionMessage}", exception.Message);
                }

                nextUtc = _timeProvider.GetUtcDateTimeNow().AddMinutes(DelayMinutes);
            }

            if (nextUtc.HasValue)
            {
                logger.LogInformation("Next database cleanup scheduled for {NextBackupTimeUtc:O} (UTC)", nextUtc);

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
