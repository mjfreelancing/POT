using Pot.App.Features.Settings.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Settings;

public interface ISettingsService : IPotScopedDependency
{
    Task<EmailBudgetReminderSettings> GetEmailBudgetReminderSettingsAsync(CancellationToken cancellationToken);
}
