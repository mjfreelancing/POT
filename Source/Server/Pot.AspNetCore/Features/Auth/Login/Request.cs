using System.ComponentModel;

namespace Pot.AspNetCore.Features.Auth.Login;

/// <summary>Carries the credentials and request context needed to sign in a user.</summary>
public sealed class Request
{
    /// <summary>The user's username</summary>
    [Description("The user's username")]
    public required string Username { get; set; }

    /// <summary>The user's password</summary>
    [Description("The user's password")]
    public required string Password { get; set; }
}
