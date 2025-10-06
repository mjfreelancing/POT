using AllOverIt.Assertion;
using AllOverIt.IO;
using AllOverIt.Logging.Extensions;
using Pot.App.Concerns.Time;

namespace Pot.AspNetCore.Features.DbBackup;

internal sealed class BackupFileCleaner : IBackupFileCleaner
{
    private readonly ITimeProvider _timeProvider;
    private readonly ILogger<BackupFileCleaner> _logger;

    public BackupFileCleaner(ITimeProvider timeProvider, ILogger<BackupFileCleaner> logger)
    {
        _timeProvider = timeProvider.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public Task RemoveOldFilesAsync(string path, int retentionDays, CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { path, retentionDays });

        var files = FileSearch
            .GetFiles(path, "*.backup", DiskSearchOptions.IgnoreUnauthorizedException | DiskSearchOptions.IgnoreIoException, cancellationToken)
            .OrderBy(file => file.CreationTimeUtc)
            .ToArray();

        var minimumToKeepUtc = _timeProvider
            .GetUtcDateTimeNow()
            .AddDays(-retentionDays);

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (file.CreationTimeUtc < minimumToKeepUtc)
            {
                _logger.LogInformation("Deleting old backup file {FileName} created on {LastWriteTimeUtc:O} (UTC)", file.Name, file.CreationTimeUtc);
                file.Delete();
            }
            else
            {
                _logger.LogInformation("Retaining backup file {FileName} created on {LastWriteTimeUtc:O} (UTC)", file.Name, file.CreationTimeUtc);
            }
        }

        return Task.CompletedTask;
    }
}
