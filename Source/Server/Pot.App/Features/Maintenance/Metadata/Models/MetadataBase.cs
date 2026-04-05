namespace Pot.App.Features.Maintenance.Metadata.Models;

public abstract class MetadataBase
{
    public const int CurrentVersion = 2;

    public abstract int Version { get; }
    public required DateTime CreatedAt { get; init; }
}
