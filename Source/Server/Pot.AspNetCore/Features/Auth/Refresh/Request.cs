using System.ComponentModel;

namespace Pot.AspNetCore.Features.Auth.Refresh;

public sealed class Request
{
    [Description("The user's refresh token")]
    public required string RefreshToken { get; set; }
}
