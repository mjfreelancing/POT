using Pot.Shared.Enumerations;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Approvals.UpdateStatus;

/// <summary>Carries the values needed to update the status of an approval.</summary>
public sealed class Request
{
    /// <summary>The User's entity tag</summary>
    [Description("The User's entity tag")]
    public long Etag { get; init; }

    /// <summary>The approval status to apply to the user</summary>
    [Description("The approval status to apply to the user")]
    public required ApprovalStatus Status { get; init; }  // Serialized via EnrichedEnumJsonConverter<ApprovalStatus>
}
