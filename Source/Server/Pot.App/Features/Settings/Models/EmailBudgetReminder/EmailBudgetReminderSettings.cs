using Pot.Shared.Enumerations;

namespace Pot.App.Features.Settings.Models.EmailBudgetReminder;

// ============================================================================================================
// DESIGN RATIONALE: Why This Class Is Partial
// ============================================================================================================
//
// This class is split across three partial class files purely for READABILITY and ORGANIZATION:
//   1. EmailBudgetReminderSettings.cs (this file)      - Properties only (the "model")
//   2. EmailBudgetReminderSettings.Metadata.cs         - Metadata (defaults, descriptions, type resolvers)
//   3. EmailBudgetReminderSettings.Validation.cs       - Validation logic and interface implementation
//
// WHY ALL THREE CONCERNS MUST LIVE IN THE SAME CLASS:
//
// The ISettingValueValidatable interface uses a static abstract method pattern (C# 11+ feature):
//     static abstract bool ValidateValue(string keyName, string stringValue);
//
// This forces the validation logic to be implemented directly on the settings model class itself.
// There's no way to separate validation into a standalone validator class while still satisfying
// the interface contract that enables compile-time polymorphism.
//
// THE DESIGN TRADE-OFF:
//
// While this creates a class with mixed responsibilities (model + validation + metadata), it provides
// significant benefits:
//   - Single source of truth: All EmailBudgetReminder settings concerns are colocated
//   - Type safety: The static abstract pattern enables type-safe, compile-time validation routing
//   - High cohesion: Changes to properties require updates to metadata and validation anyway
//   - Zero runtime overhead: No reflection, no dependency injection, pure static dispatch
//
// The partial class approach gives us the best of both worlds:
//   - Satisfies the original intent (main file contains just properties)
//   - Maintains all the benefits of the static abstract interface pattern
//   - Improves navigability (each file has a clear, focused purpose)
//   - Follows .NET conventions (similar to Entity Framework's code generation patterns)
//
// VALIDATION CALL CHAIN:
//
// When a setting value is validated, the call flows through:
//   UpdateSettingService.ValidateSettingValue(category, key, value)
//     └─> SettingValueValidators[category] lookup
//         └─> ValidateSettingValue<EmailBudgetReminderSettings>(key, value)  [generic method]
//             └─> EmailBudgetReminderSettings.ValidateValue(key, value)  [static interface method]
//                 └─> ValueValidators[key](value)  [actual validation logic]
//
// This multi-level design ensures:
//   - Category-level type safety (only valid setting classes can be registered)
//   - Key-level validation (each setting key has specific type and range checks)
//   - Business rule enforcement (e.g., hours 0-23, days 0-31)
//
// ============================================================================================================

/// <summary>
/// Settings model for email budget reminders.
/// Corresponds to <see cref="SettingCategory.EmailBudgetReminder"/>.
/// Implements <see cref="ISettingValueValidatable"/> to provide category-specific validation.
/// </summary>
/// <remarks>
/// This is a partial class. See the comment block above for design rationale.
/// See also: EmailBudgetReminderSettings.Metadata.cs and EmailBudgetReminderSettings.Validation.cs
/// </remarks>
public sealed partial class EmailBudgetReminderSettings : ISettingValueValidatable
{
    public required bool Enabled { get; init; }
    public required int ReminderDays { get; init; }
    public required int LocalHourTrigger { get; init; }
}
