using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Extensions;
using Pot.App.Features.Settings.Models;
using Pot.Data.Repositories.Settings;

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

    public async Task<EmailUpcomingReminderSettings> GetEmailUpcomingReminderSettingsAsync(CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        // This is auto-filtered to the user's site
        var settings = await _settingsRepository
            .GetEmailUpcomingRemindersAsync(cancellationToken)
            .ConfigureAwait(false);

        var keyedSettings = settings.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        return new EmailUpcomingReminderSettings
        {
            Enabled = keyedSettings.GetBool(nameof(EmailUpcomingReminderSettings.Enabled), false),
            ReminderDays = keyedSettings.GetInt(nameof(EmailUpcomingReminderSettings.ReminderDays), 7),
            LocalHourTrigger = keyedSettings.GetInt(nameof(EmailUpcomingReminderSettings.LocalHourTrigger), 6)
        };
    }
}
