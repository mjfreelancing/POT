using Pot.App.Features.Users.Invite.Models;

namespace Pot.App.Features.Users.Invite.EntityChecks;

/// <summary>
/// Carries the state evaluated by the pre-invite checks during a user invitation.
/// </summary>
internal sealed class InputState
{
    /// <summary>
    /// Gets the requested invitation.
    /// </summary>
    public required Input Input { get; init; }

    /// <summary>
    /// Gets the role identifiers requested for the invited user.
    /// </summary>
    public required Guid[] RoleIds { get; init; }
}
