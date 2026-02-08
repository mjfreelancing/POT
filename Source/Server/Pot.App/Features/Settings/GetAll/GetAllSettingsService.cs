using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Features.Settings.GetAll.Models;
using Pot.App.Features.Settings.Models;
using Pot.Data.Repositories.Settings;
using Pot.Shared.Enumerations;

namespace Pot.App.Features.Settings.GetAll;

internal sealed class GetAllSettingsService : IGetAllSettingsService
{
    private static readonly Dictionary<SettingCategory, Func<Dictionary<string, SettingValueMetadata>>> DefaultSettingsRegistry = new()
    {
        { SettingCategory.EmailBudgetReminder, () => EmailBudgetReminderSettings.Defaults }
    };

    private readonly ISettingsRepository _settingsRepository;
    private readonly ILogger _logger;

    public GetAllSettingsService(ISettingsRepository settingsRepository, ILogger<GetAllSettingsService> logger)
    {
        _settingsRepository = settingsRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<Output>> GetAllSettingsAsync(CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var allSettings = await _settingsRepository
            .GetAllSettingsAsync(cancellationToken)
            .ConfigureAwait(false);

        var settingsByCategory = allSettings
            .GroupBy(setting => setting.Category)
            .ToDictionary(grp => grp.Key, grp => grp.ToArray());

        var categories = new List<CategorySettings>();

        foreach (var category in SettingCategory.GetAll())
        {
            var dbSettings = settingsByCategory.GetValueOrDefault(category) ?? [];
            var defaults = GetCategoryDefaults(category);

            var mergedSettings = new CategorySettings.SettingItems();
            var dbSettingsByKey = dbSettings.ToDictionary(setting => setting.Key);

            foreach (var (settingName, valueMetadata) in defaults)
            {
                var dbSetting = dbSettingsByKey.GetValueOrDefault(settingName);

                mergedSettings[settingName] = dbSetting is not null
                    ? new CategorySettings.SettingItem
                    {
                        RowId = dbSetting.RowId,
                        Etag = dbSetting.Etag,
                        Value = valueMetadata.ValueResolver.Invoke(dbSetting.Value),     // Convert string value from DB to the correct type using the resolver function
                        Description = valueMetadata.Description
                    }
                    : new CategorySettings.SettingItem
                    {
                        Value = valueMetadata.DefaultValue,
                        Description = valueMetadata.Description
                    };
            }

            categories.Add(new CategorySettings
            {
                Category = category.Name,
                Settings = mergedSettings
            });
        }

        var output = new Output
        {
            Categories = [.. categories]
        };

        return EnrichedResult.Success(output);
    }

    // Mapping of category to a function that returns the default settings for that category.
    // This allows us to keep the logic for determining defaults close to the relevant code
    // and makes it easy to add new categories in the future.
    private static Dictionary<string, SettingValueMetadata> GetCategoryDefaults(SettingCategory category)
    {
        if (DefaultSettingsRegistry.TryGetValue(category, out var resolver))
        {
            return resolver();
        }

        return [];
    }
}
