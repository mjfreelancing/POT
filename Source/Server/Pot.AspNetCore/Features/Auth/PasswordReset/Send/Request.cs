using System.ComponentModel;

namespace Pot.AspNetCore.Features.Auth.PasswordReset.Send;

public sealed class Request
{
    [Description("The user's username")]
    public required string Username { get; set; }
}
