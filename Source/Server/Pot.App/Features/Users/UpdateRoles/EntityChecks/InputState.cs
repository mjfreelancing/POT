using Pot.App.Features.Users.UpdateRoles.Models;

namespace Pot.App.Features.Users.UpdateRoles.EntityChecks;

internal sealed class InputState
{
    public required Input Input { get; init; }

    // Even though it's the roles being updated, we're using the user's etag for concurrency checks
    public required long UserEtag { get; init; }

    public required Guid[] RoleIds { get; init; }
}
