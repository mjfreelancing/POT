using Pot.App.Errors;

namespace Pot.App.Features.Settings.Models;

/// <summary>
/// Interface for setting types that provide value validation.
/// Implementing classes define category-specific validation rules for setting keys and values.
/// </summary>
/// <remarks>
/// This interface uses static abstract members (C# 11+ feature) to enable compile-time polymorphism
/// without requiring instance creation.
/// </remarks>
public interface ISettingValueValidatable
{
    /// <summary>
    /// Validates that a string value is valid for a specific setting key.
    /// Validation includes type checking (e.g., can parse to bool/int), range validation,
    /// and any business rule constraints specific to the setting.
    /// </summary>
    /// <param name="keyName">The setting key name (e.g., "Enabled", "ReminderDays")</param>
    /// <param name="stringValue">The raw string value to validate</param>
    /// <returns><see cref="ProblemDetailsError"/> if the value is invalid for the specified key, <see langword="null"/> otherwise</returns>    /// 
    /// <exception cref="UnreachableException">Thrown when the keyName is not recognized for the setting category</exception>
    /// <remarks>
    /// This method is called during API requests to validate user-provided setting values before database persistence.
    /// </remarks>
    static abstract ProblemDetailsError? ValidateValue(string keyName, string stringValue);
}
