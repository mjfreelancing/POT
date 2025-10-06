namespace Pot.App.Features.Maintenance.Metadata.Models;

public abstract class MetadataBase
{
    public abstract int Version { get; }
    public required DateTime CreatedAt { get; init; }
}
