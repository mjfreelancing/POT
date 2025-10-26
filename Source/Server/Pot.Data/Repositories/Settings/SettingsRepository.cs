using AllOverIt.Extensions;
using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;
using Pot.Data.Repositories.Settings.Extensions;
using Pot.Data.Repositories.Settings.Models;
using Pot.Shared;

namespace Pot.Data.Repositories.Settings;

internal sealed class SettingsRepository : RepositoryBase, ISettingsRepository
{
    public IQueryable<SettingEntity> Settings => _dbContext.Settings;

    public SettingsRepository(PotDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<BackupSettings> GetDatabaseSettingsAsync(CancellationToken cancellationToken)
    {
        var settings = await Settings
            .IgnoreQueryFilters()
            .Where(setting => setting.Category == SettingCategory.Backup)
            .Select(setting => new
            {
                setting.Key,
                setting.Value
            })
            .ToListAsync(cancellationToken);

        var keyedSettings = settings.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        return new BackupSettings
        {
            Enabled = keyedSettings.GetBool(nameof(BackupSettings.Enabled), false),
            RetentionDays = keyedSettings.GetInt(nameof(BackupSettings.RetentionDays), 7),
            Schedule = keyedSettings.GetString(nameof(BackupSettings.Schedule), "0 */6 * * *") // Default: every 6 hours
        };
    }
}
