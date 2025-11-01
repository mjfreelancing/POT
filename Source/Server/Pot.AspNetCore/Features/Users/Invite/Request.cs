using System.ComponentModel;

namespace Pot.AspNetCore.Features.Users.Invite;

public sealed class Request
{
    [Description("The user's username")]
    public required string Username { get; set; }

    [Description("The user's email")]
    public required string Email { get; set; }

    [Description("The user roles")]
    public required Guid[] RoleIds { get; set; }
}
