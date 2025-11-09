using Pot.Shared.Enumerations;

namespace Pot.App.Features.Approvals.UpdateStatus.Models;

public sealed class Input
{
    public Guid RowId { get; init; }
    public long Etag { get; init; }
    public required ApprovalStatus Status { get; init; }
}
