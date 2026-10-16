using System.ComponentModel;

namespace Pot.AspNetCore.Features.Users.UpdateRoles;

/// <summary>Carries the values needed to update roles for a user.</summary>
public sealed class Request
{
    /// <summary>The users's entity tag</summary>
    [Description("The users's entity tag")]
    public long Etag { get; init; }

    /// <summary>The role Ids to assign to the user</summary>
    [Description("The role Ids to assign to the user")]
    public required IReadOnlyList<Guid> RoleIds { get; init; }
}
