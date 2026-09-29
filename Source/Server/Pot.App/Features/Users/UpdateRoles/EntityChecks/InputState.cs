using Pot.App.Features.Users.UpdateRoles.Models;

namespace Pot.App.Features.Users.UpdateRoles.EntityChecks;

/// <summary>
/// Carries the state evaluated by the pre-update checks during a user role change.
/// </summary>
internal sealed class InputState
{
    /// <summary>
    /// Gets the requested role change.
    /// </summary>
    public required Input Input { get; init; }

    // Even though it's the roles being updated, we're using the user's etag for concurrency checks
    /// <summary>
    /// Gets the concurrency eTag of the stored user whose roles are changing.
    /// </summary>
    public required long UserEtag { get; init; }

    /// <summary>
    /// Gets the role identifiers to assign to the user.
    /// </summary>
    public required Guid[] RoleIds { get; init; }
}
