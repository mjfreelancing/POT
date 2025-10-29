namespace Pot.App.Features.Users.UpdateRoles.Models;

public sealed class Input
{
    public Guid RowId { get; init; }
    public long Etag { get; init; }
    public required Guid[] RoleIds { get; init; }
}
