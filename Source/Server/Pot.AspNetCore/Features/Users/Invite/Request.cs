using System.ComponentModel;

namespace Pot.AspNetCore.Features.Users.Invite;

/// <summary>Carries the values needed to invite a user.</summary>
public sealed class Request
{
    /// <summary>The user's username</summary>
    [Description("The user's username")]
    public required string Username { get; set; }

    /// <summary>The user's email</summary>
    [Description("The user's email")]
    public required string Email { get; set; }

    /// <summary>The user roles</summary>
    [Description("The user roles")]
    public required Guid[] RoleIds { get; set; }
}
