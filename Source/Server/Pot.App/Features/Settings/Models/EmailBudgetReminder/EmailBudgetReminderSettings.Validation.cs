using Pot.App.Extensions;
using System.Diagnostics;

namespace Pot.App.Features.Settings.Models.EmailBudgetReminder;

/// <summary>
/// Validation logic for EmailBudgetReminder settings.
/// This partial class implements the ISettingValueValidatable interface requirement.
/// </summary>
public sealed partial class EmailBudgetReminderSettings
{
    /// <summary>
    /// Validation functions for each setting key in the EmailBudgetReminder category.
    /// Each validator checks:
    /// - Type validity: Can the string be parsed to the expected type?
    /// - Range validity: Is the value within acceptable bounds?
    /// - Business rules: Does the value meet domain-specific constraints?
    /// </summary>
    /// <remarks>
    /// Validation rules:
    /// - Enabled: Must be parseable to boolean (case-insensitive "true"/"false")
    /// - LocalHourTrigger: Must be an integer between 0-23 (valid hours in a day)
    /// - ReminderDays: Must be an integer between 0-31 (reasonable reminder period)
    /// </remarks>
    private static readonly Dictionary<string, Func<string, bool>> ValueValidators = new()
    {
        [nameof(Enabled)] = stringValue => stringValue.TryAsBoolean(out _),
        [nameof(LocalHourTrigger)] = stringValue => stringValue.TryAsInt(out int value) && value >= 0 && value < 24,
        [nameof(ReminderDays)] = stringValue => stringValue.TryAsInt(out var value) && value >= 0 && value <= 31
    };

    /// <summary>
    /// Validates a setting value for a specific key in the EmailBudgetReminder category.
    /// Implementation of <see cref="ISettingValueValidatable.ValidateValue"/>.
    /// </summary>
    /// <param name="keyName">The setting key name (e.g., "Enabled", "ReminderDays", "LocalHourTrigger")</param>
    /// <param name="value">The string value to validate</param>
    /// <returns>True if the value passes validation for the specified key, false otherwise</returns>
    /// <exception cref="UnreachableException">Thrown when keyName is not a recognized setting key for this category</exception>
    /// <remarks>
    /// This method is called during:
    /// - API requests (UpdateSettingService) to validate user-provided values before database persistence
    /// - Application startup to validate that default setting values are correctly defined
    /// 
    /// The validation is invoked through the following call chain:
    /// UpdateSettingService → SettingValueValidators[category] → ValidateSettingValue&lt;T&gt; → this method
    /// </remarks>
    public static bool ValidateValue(string keyName, string value)
    {
        return ValueValidators.TryGetValue(keyName, out var validator)
            ? validator.Invoke(value)
            : throw new UnreachableException($"Unknown setting key name '{keyName}' for category '{nameof(EmailBudgetReminderSettings)}'");
    }
}
