using Pot.Shared.Enumerations;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Users.UpdateStatus;

public sealed class Request
{
    [Description("The users's entity tag")]
    public long Etag { get; init; }

    [Description("The user's status")]
    public required UserStatus Status { get; init; }
}
