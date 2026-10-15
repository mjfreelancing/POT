using Pot.Shared.Enumerations;

namespace Pot.App.Features.Users.UpdateStatus.Models;

public sealed class Input
{
    public Guid RowId { get; init; }
    public long Etag { get; init; }
    public required UserStatus Status { get; init; }

    // The configured platform administrators, taken from deployment configuration rather than the database, so
    // the checks that protect them do not need to resolve the configuration themselves.
    public required Guid[] PlatformAdminRowIds { get; init; }
}
