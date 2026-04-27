using Pot.Shared.Enumerations;

namespace Pot.App.Features.Settings.Upsert.Models;

public sealed class Input
{
    public required SettingCategory Category { get; init; }
    public required string Key { get; init; }
    public required string Value { get; init; }
    public required long? Etag { get; init; }
}
