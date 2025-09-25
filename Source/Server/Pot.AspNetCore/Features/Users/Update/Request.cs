using System.ComponentModel;

namespace Pot.AspNetCore.Features.Users.Update;

public sealed class Request
{
    [Description("The users's entity tag")]
    public long Etag { get; init; }

    [Description("The user's display name")]
    public required string DisplayName { get; init; }

    [Description("The user's email")]
    public required string Email { get; init; }
}
