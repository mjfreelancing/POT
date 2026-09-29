using Pot.App.Features.Settings.Models.EmailBudgetReminder;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Settings;

/// <summary>
/// Provides the email budget reminder settings for the current site.
/// </summary>
public interface ISettingsService : IPotScopedDependency
{
    /// <summary>
    /// Gets the email budget reminder settings for the current site, falling back to defaults for any setting that is
    /// not stored.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The email budget reminder settings for the current site.</returns>
    Task<EmailBudgetReminderSettings> GetEmailBudgetReminderSettingsAsync(CancellationToken cancellationToken);
}
