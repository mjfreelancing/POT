using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Extensions;
using Pot.App.Features.Settings.Models.EmailBudgetReminder;
using Pot.Data.Repositories.Settings;
using Pot.Shared.Enumerations;

namespace Pot.App.Features.Settings;

internal sealed class SettingsService : ISettingsService
{
    private readonly ISettingsRepository _settingsRepository;
    private readonly ILogger<SettingsService> _logger;

    public SettingsService(ISettingsRepository settingsRepository, ILogger<SettingsService> logger)
    {
        _settingsRepository = settingsRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EmailBudgetReminderSettings> GetEmailBudgetReminderSettingsAsync(CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        // This is auto-filtered to the user's site
        var settings = await _settingsRepository
            .GetSettingsForCategoryAsync(SettingCategory.EmailBudgetReminder, cancellationToken)
            .ConfigureAwait(false);

        var keyedSettings = settings.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        return new EmailBudgetReminderSettings
        {
            Enabled = keyedSettings.GetBool(nameof(EmailBudgetReminderSettings.Enabled), false),
            ReminderDays = keyedSettings.GetInt(nameof(EmailBudgetReminderSettings.ReminderDays), 7),
            LocalHourTrigger = keyedSettings.GetInt(nameof(EmailBudgetReminderSettings.LocalHourTrigger), 6)
        };
    }
}
