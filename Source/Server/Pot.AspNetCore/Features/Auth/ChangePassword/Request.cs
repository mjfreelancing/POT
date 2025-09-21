using System.ComponentModel;

namespace Pot.AspNetCore.Features.Auth.ChangePassword;

public sealed class Request
{
    [Description("The current password")]
    public required string CurrentPassword { get; set; }

    [Description("The new password")]
    public required string NewPassword { get; set; }
}
