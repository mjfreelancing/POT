using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Notifications.BudgetReminder;

public interface IBudgetReminderService : IPotScopedDependency
{
    // Send reminder emails for the current user if the reminders are enabled and the current hour matches the configured hour.
    Task SendRemindersAsync(CancellationToken cancellationToken);
}
