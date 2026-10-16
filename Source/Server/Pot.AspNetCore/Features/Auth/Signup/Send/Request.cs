using System.ComponentModel;

namespace Pot.AspNetCore.Features.Auth.Signup.Send;

/// <summary>Carries the values needed to start a user registration request.</summary>
public sealed class Request
{
    /// <summary>The user's username</summary>
    [Description("The user's username")]
    public required string Username { get; set; }

    /// <summary>The user's email</summary>
    [Description("The user's email")]
    public required string Email { get; set; }
}
