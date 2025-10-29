using System.ComponentModel;

namespace Pot.AspNetCore.Features.Users.UpdateRoles;

public sealed class Request
{
    [Description("The users's entity tag")]
    public long Etag { get; init; }

    [Description("The role Ids to assign to the user")]
    public required Guid[] RoleIds { get; init; }
}
