using Pot.Data.Entities;
using Pot.Data.Repositories.Settings.Models;

namespace Pot.Data.Repositories.Settings;

public interface ISettingsRepository : IGenericRepository<PotDbContext, SettingEntity>
{
    Task<BackupSettings> GetDatabaseSettingsAsync(CancellationToken cancellationToken);
}
