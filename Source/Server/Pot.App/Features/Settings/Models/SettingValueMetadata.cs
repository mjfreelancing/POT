namespace Pot.App.Features.Settings.Models;

/// <summary>
/// Metadata for a single setting key, defining its default value, description, and type conversion logic.
/// This is used in conjunction with validation to provide a complete setting definition.
/// </summary>
public sealed class SettingValueMetadata
{
    /// <summary>
    /// The default value for the setting, used when no value exists in the database.
    /// Type should match the resolved type from ValueResolver.
    /// </summary>
    public required object DefaultValue { get; init; }

    /// <summary>
    /// Human-readable description of what the setting controls.
    /// Used for documentation and API responses.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Function to convert the string value from the database to the correct typed value.
    /// Ensures API responses contain properly typed values (e.g., bool, int) rather than strings.
    /// </summary>
    /// <remarks>
    /// Used during setting retrieval (GET operations) to deserialize stored string values.
    /// Complementary to the validation system which ensures only valid strings are stored.
    /// </remarks>
    public required Func<string, object> ValueResolver { get; init; }
}
