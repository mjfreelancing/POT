using Pot.Shared.DependencyInjection;

namespace Pot.AspNetCore.Features.DbBackup;

public interface IBackupFileCleaner : IPotScopedDependency
{
    Task RemoveOldFilesAsync(string path, int retentionDays, CancellationToken cancellationToken);
}
