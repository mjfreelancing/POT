namespace Pot.App.Features.Settings.Models;

public sealed record SettingValueMetadata
{
    public required object DefaultValue { get; init; }
    public required string Description { get; init; }

    // Used for converting string values from the database to the correct type for the setting - e.g. bool, int, etc.
    // This ensures values returned by the API result in a serialized value of the correct type, rather than always being a string
    public required Func<string, object> ValueResolver { get; init; }
}
