using Pot.Shared.Enumerations;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Approvals.UpdateStatus;

public sealed class Request
{
    [Description("The User's entity tag")]
    public long Etag { get; init; }

    [Description("The approval status to apply to the user")]
    public required ApprovalStatus Status { get; init; }  // Serialized via EnrichedEnumJsonConverter<ApprovalStatus>
}
