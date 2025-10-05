using Pot.Data.Repositories.Settings.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.AspNetCore.Features.DbBackup;

public interface IPostgresqlBackup : IPotScopedDependency
{
    Task<BackupSettings> GetBackupSettingsAsync(CancellationToken cancellationToken);
    Task ExecuteAsync(string backupPath, CancellationToken cancellationToken);
}
