namespace Pot.App.Features.Sites.Update.Models;

public sealed class Input
{
    public Guid RowId { get; init; }
    public long Etag { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
}
