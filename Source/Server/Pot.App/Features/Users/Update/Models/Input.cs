namespace Pot.App.Features.Users.Update.Models;

public sealed class Input
{
    public Guid RowId { get; init; }
    public long Etag { get; init; }
    public required string DisplayName { get; init; }
    public required string Email { get; init; }
}
