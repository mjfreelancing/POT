using Pot.App.Extensions;
using Pot.Shared.Enumerations;

namespace Pot.App.Features.Settings.Models.EmailBudgetReminder;

/// <summary>
/// Metadata definitions for EmailBudgetReminder settings.
/// This partial class contains default values, descriptions, and type conversion logic.
/// </summary>
public sealed partial class EmailBudgetReminderSettings
{
    /// <summary>
    /// Metadata for all settings in the <see cref="SettingCategory.EmailBudgetReminder"/> category.
    /// Defines default values, descriptions, and type conversion logic for each setting key.
    /// Used by the settings retrieval system to return properly typed values from string storage.
    /// </summary>
    /// <remarks>
    /// The ValueResolver functions are used during GET operations to deserialize stored string values
    /// into the correct types (bool, int, etc.). This ensures API responses contain properly typed values
    /// rather than strings. These resolvers are complementary to the validation system, which ensures
    /// only valid strings are stored in the first place.
    /// </remarks>
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
}
