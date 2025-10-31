using Pot.Shared.Enumerations;

namespace Pot.App.Features.Users.UpdateStatus.Models;

public sealed class Input
{
    public Guid RowId { get; init; }
    public long Etag { get; init; }
    public required UserStatus Status { get; init; }
}
