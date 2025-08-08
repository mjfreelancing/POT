namespace Pot.App.Features.Accounts.Update.Models;

public sealed class Input
{
    public Guid RowId { get; init; }
    public long Etag { get; init; }
    public bool ExcludeFromCalcs { get; init; }
    public required string Bsb { get; init; }
    public required string Number { get; init; }
    public required string Description { get; init; }
    public double Balance { get; init; }
    public double Reserved { get; init; }
}
