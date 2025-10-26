using Pot.Data.Repositories.Settings.Models;

namespace Pot.Data.Repositories.Settings;

public interface ISettingsRepository : IRepositoryBase
{
    Task<BackupSettings> GetDatabaseSettingsAsync(CancellationToken cancellationToken);
}
