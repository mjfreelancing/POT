using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Notifications.BudgetReminder;

/// <summary>
/// Sends budget reminder emails to users whose reminder settings are due.
/// </summary>
public interface IBudgetReminderService : IPotScopedDependency
{
    /// <summary>
    /// Sends the current user a budget reminder when reminders are enabled and the configured local hour matches
    /// the current local hour.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    // Send reminder emails for the current user if the reminders are enabled and the current hour matches the configured hour.
    Task SendRemindersAsync(CancellationToken cancellationToken);
}
