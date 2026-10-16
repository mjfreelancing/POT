using System.ComponentModel;

namespace Pot.AspNetCore.Features.Me.ChangePassword;

/// <summary>Carries the values needed to change the password for the current user.</summary>
public sealed class Request
{
    /// <summary>The current password</summary>
    [Description("The current password")]
    public required string CurrentPassword { get; set; }

    /// <summary>The new password</summary>
    [Description("The new password")]
    public required string NewPassword { get; set; }
}
