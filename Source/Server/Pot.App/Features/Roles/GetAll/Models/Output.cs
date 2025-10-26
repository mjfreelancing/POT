namespace Pot.App.Features.Roles.GetAll.Models;

public sealed class Output
{
    public required Guid RowId { get; init; }
    public required long Etag { get; init; }
    public required string Name { get; init; }
}
