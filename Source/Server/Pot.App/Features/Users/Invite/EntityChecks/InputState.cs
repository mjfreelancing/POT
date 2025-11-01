using Pot.App.Features.Users.Invite.Models;

namespace Pot.App.Features.Users.Invite.EntityChecks;

internal sealed class InputState
{
    public required Input Input { get; init; }
    public required Guid[] RoleIds { get; init; }
}
