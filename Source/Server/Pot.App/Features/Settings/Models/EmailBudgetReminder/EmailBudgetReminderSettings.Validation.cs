using Pot.App.Errors;
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
    /// 
    /// Returns null if validation passes, otherwise returns a ProblemDetailsError with details about the failure.
    /// </remarks>
    private static readonly Dictionary<string, Func<string, ProblemDetailsError?>> ValueValidators = new()
    {
        [nameof(Enabled)] = stringValue =>
        {
            if (!stringValue.TryAsBoolean(out _))
            {
                return ProblemDetailsErrorFactory.CreateUnprocessableEntityError(
                    nameof(Enabled),
                    stringValue,
                    "Value must be 'true' or 'false'");
            }

            return null;
        },

        [nameof(LocalHourTrigger)] = stringValue =>
        {
            if (!stringValue.TryAsInt(out int value) || value < 0 || value >= 24)
            {
                return ProblemDetailsErrorFactory.CreateUnprocessableEntityError(
                    nameof(LocalHourTrigger),
                    stringValue,
                    "Value must be between 0 and 23 (inclusive)");
            }

            return null;
        },

        [nameof(ReminderDays)] = stringValue =>
        {
            if (!stringValue.TryAsInt(out var value) || value < 0 || value > 31)
            {
                return ProblemDetailsErrorFactory.CreateUnprocessableEntityError(
                    nameof(ReminderDays),
                    stringValue,
                    "Value must be between 0 and 31 (inclusive)");
            }

            return null;
        }
    };

    /// <summary>
    /// Validates a setting value for a specific key in the EmailBudgetReminder category.
    /// Implementation of <see cref="ISettingValueValidatable.ValidateValue"/>.
    /// </summary>
    /// <param name="keyName">The setting key name (e.g., "Enabled", "ReminderDays", "LocalHourTrigger")</param>
    /// <param name="value">The string value to validate</param>
    /// <returns><see cref="ProblemDetailsError"/> if the value is invalid for the specified key, <see langword="null"/> otherwise</returns>    /// 
    /// <exception cref="UnreachableException">Thrown when keyName is not a recognized setting key for this category</exception>
    /// <remarks>
    /// This method is called during:
    /// - API requests (UpdateSettingService) to validate user-provided values before database persistence
    /// - Application startup to validate that default setting values are correctly defined
    /// 
    /// The validation is invoked through the following call chain:
    /// UpdateSettingService → SettingValueValidators[category] → ValidateSettingValue&lt;T&gt; → this method
    /// </remarks>
    public static ProblemDetailsError? ValidateValue(string keyName, string value)
    {
        // There's no benefit trying to move this method to ISettingValueValidatable because:
        // 1. C# static interface members cannot access implementing type's static members (ValueValidators)
        // 2. Each implementation must provide its own ValueValidators dictionary anyway
        // 3. Error messages need the specific category name (nameof(EmailBudgetReminderSettings))
        // 4. This keeps the implementation explicit, clear, and maintainable (KISS principle)
        // 5. The "duplication" is only 3 lines per setting category - negligible cost for clarity
        return ValueValidators.TryGetValue(keyName, out var validator)
            ? validator.Invoke(value)
            : throw new UnreachableException($"Unknown setting key name '{keyName}' for category '{nameof(EmailBudgetReminderSettings)}'");
    }
}
