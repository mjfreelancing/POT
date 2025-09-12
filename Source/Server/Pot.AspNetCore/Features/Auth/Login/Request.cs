using System.ComponentModel;

namespace Pot.AspNetCore.Features.Auth.Login;

public sealed class Request
{
    [Description("The user's username")]
    public required string Username { get; set; }

    [Description("The user's password")]
    public required string Password { get; set; }
}
