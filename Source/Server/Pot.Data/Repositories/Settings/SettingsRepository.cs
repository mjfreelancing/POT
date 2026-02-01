using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;
using Pot.Shared.Enumerations;

namespace Pot.Data.Repositories.Settings;

internal sealed class SettingsRepository : RepositoryBase, ISettingsRepository
{
    public IQueryable<SettingEntity> Settings => _dbContext.Settings;

    public SettingsRepository(PotDbContext dbContext)
        : base(dbContext)
    {
    }

    public Task<List<SettingEntity>> GetEmailUpcomingRemindersAsync(CancellationToken cancellationToken)
    {
        // This is auto-filtered to the current site (based on the current user)
        return Settings
            .Where(setting => setting.Category == SettingCategory.EmailUpcomingReminders)
            .ToListAsync(cancellationToken);
    }
}
