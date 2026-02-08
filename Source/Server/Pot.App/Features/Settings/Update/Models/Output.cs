namespace Pot.App.Features.Settings.Update.Models;

public sealed class Output
{
    public required Guid RowId { get; init; }
    public required long Etag { get; init; }
}