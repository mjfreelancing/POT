namespace Pot.App.Features.Maintenance.Metadata.Models;

internal sealed class MetadataV1 : MetadataBase
{
    public override int Version => 1;
    public required DateTime CreatedAt { get; init; }
}
