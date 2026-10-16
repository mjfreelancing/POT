using Pot.Shared.Enumerations;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Users.UpdateStatus;

/// <summary>Carries the values needed to update the status of a user.</summary>
public sealed class Request
{
    /// <summary>The users's entity tag</summary>
    [Description("The users's entity tag")]
    public long Etag { get; init; }

    /// <summary>The user's status</summary>
    [Description("The user's status")]
    public required UserStatus Status { get; init; }
}
