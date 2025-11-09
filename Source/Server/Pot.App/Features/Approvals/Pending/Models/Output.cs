namespace Pot.App.Features.Approvals.Pending.Models;

public sealed class Output
{
    public required Guid RowId { get; init; }
    public required long Etag { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
}
