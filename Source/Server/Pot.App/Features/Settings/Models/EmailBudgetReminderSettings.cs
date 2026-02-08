using Pot.App.Extensions;

namespace Pot.App.Features.Settings.Models;

// Default setting values for the category: SettingCategory.EmailBudgetReminders
public sealed class EmailBudgetReminderSettings
{
    internal static Dictionary<string, SettingValueMetadata> Defaults = new()
    {
        [nameof(Enabled)] = new SettingValueMetadata
        {
            DefaultValue = false,
            Description = "Enable email budget reminders",
            ValueResolver = stringValue => stringValue.AsBoolean(false)
        },
        [nameof(ReminderDays)] = new SettingValueMetadata
        {
            DefaultValue = 7,
            Description = "Days before budget period to send reminder",
            ValueResolver = stringValue => stringValue.AsInt(7)
        },
        [nameof(LocalHourTrigger)] = new SettingValueMetadata
        {
            DefaultValue = 6,
            Description = "Hour of day (local time) to trigger reminder",
            ValueResolver = stringValue => stringValue.AsInt(6)
        }
    };

    public required bool Enabled { get; init; }
    public required int ReminderDays { get; init; }
    public required int LocalHourTrigger { get; init; }
}
