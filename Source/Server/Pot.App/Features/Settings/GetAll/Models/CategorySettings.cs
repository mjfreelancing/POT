namespace Pot.App.Features.Settings.GetAll.Models;

public sealed record CategorySettings
{
    public sealed record SettingItem
    {
        // Will be null when not in database - default value will be provided
        public Guid? RowId { get; init; }
        public long? Etag { get; init; }

        public required object Value { get; init; }
        public required string Description { get; init; }
    }

    public sealed class SettingItems : Dictionary<string, SettingItem>;

    public required string Category { get; init; }
    public required SettingItems Settings { get; init; }
}
