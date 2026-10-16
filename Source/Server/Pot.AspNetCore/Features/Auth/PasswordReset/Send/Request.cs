using System.ComponentModel;

namespace Pot.AspNetCore.Features.Auth.PasswordReset.Send;

/// <summary>Carries the username needed to send a password-reset message.</summary>
public sealed class Request
{
    /// <summary>The user's username</summary>
    [Description("The user's username")]
    public required string Username { get; set; }
}
