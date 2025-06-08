namespace Pot.App.Features.Incomes.Update.Models;

public sealed class Output
{
    public required Guid RowId { get; init; }
    public required long Etag { get; init; }
}
