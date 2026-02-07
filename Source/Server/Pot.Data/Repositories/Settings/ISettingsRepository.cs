using Pot.Data.Entities;

namespace Pot.Data.Repositories.Settings;

public interface ISettingsRepository : IRepositoryBase
{
    Task<List<SettingEntity>> GetEmailBudgetRemindersAsync(CancellationToken cancellationToken);
}
