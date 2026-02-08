using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;
using Pot.Shared.Enumerations;

namespace Pot.Data.Repositories.Settings;

internal sealed class SettingsRepository : PersistableRepository, IPersistableSettingsRepository
{
    public IQueryable<SettingEntity> Settings => _dbContext.Settings;

    public SettingsRepository(PotDbContext dbContext)
        : base(dbContext)
    {
    }

    public Task<List<SettingEntity>> GetAllSettingsAsync(CancellationToken cancellationToken)
    {
        // This is auto-filtered to the current site (based on the current user)
        return Settings.ToListAsync(cancellationToken);
    }

    public Task<SettingEntity?> GetSettingAsync(SettingCategory category, string key, CancellationToken cancellationToken)
    {
        // This is auto-filtered to the current site (based on the current user)
        return Settings.SingleOrDefaultAsync(setting => setting.Category == category && setting.Key == key, cancellationToken);
    }

    public Task<List<SettingEntity>> GetSettingsForCategoryAsync(SettingCategory settingCategory, CancellationToken cancellationToken)
    {
        // This is auto-filtered to the current site (based on the current user)
        return Settings
            .Where(setting => setting.Category == settingCategory)
            .ToListAsync(cancellationToken);
    }
}
