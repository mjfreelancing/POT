using Pot.Data.Entities;
using Pot.Shared.Enumerations;

namespace Pot.Data.Repositories.Settings;

public interface ISettingsRepository : IRepositoryBase
{
    // Get all settings, if any, configured for the current site
    // Note: The application service layer will use default values for any settings that are not configured for the current site
    Task<List<SettingEntity>> GetAllSettingsAsync(CancellationToken cancellationToken);

    // Try to get a specific setting for the current site
    // Note: The application service layer will use a default value for any setting that is not configured for the current site
    Task<SettingEntity?> GetSettingAsync(SettingCategory category, string key, CancellationToken cancellationToken);

    // Get all email budget reminder settings, if any, configured for the current site
    Task<List<SettingEntity>> GetSettingsForCategoryAsync(SettingCategory settingCategory, CancellationToken cancellationToken);
}
