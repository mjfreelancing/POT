using Pot.App.Features.Users.UpdateRoles.Models;
using Pot.Data.Entities;

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

    /// <summary>
    /// Gets the stored user whose roles are changing, with its current roles loaded. It belongs to the caller's
    /// site, because the checks run only for a row the site-scoped lookup returned.
    /// </summary>
    public required UserEntity Target { get; init; }

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
