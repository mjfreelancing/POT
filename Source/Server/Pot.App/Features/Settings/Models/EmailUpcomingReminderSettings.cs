namespace Pot.App.Features.Settings.Models;

public sealed class EmailUpcomingReminderSettings
{
    public bool Enabled { get; init; }
    public required int ReminderDays { get; init; }
    public required int LocalHourTrigger { get; init; }
}
