using Pot.App.Features.Users.UpdateStatus.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Users.UpdateStatus.EntityChecks;

/// <summary>
/// Carries the state evaluated by the pre-update checks during a user status change.
/// </summary>
internal sealed class InputState
{
    /// <summary>
    /// Gets the requested status change.
    /// </summary>
    public required Input Input { get; init; }

    /// <summary>
    /// Gets the stored user whose status is changing. It belongs to the caller's site, because the checks run
    /// only for a row the site-scoped lookup returned.
    /// </summary>
    public required UserEntity Target { get; init; }

    /// <summary>
    /// Gets the concurrency eTag of the stored user whose status is changing.
    /// </summary>
    public required long UserEtag { get; init; }

    /// <summary>
    /// Gets the configured platform administrator ids.
    /// </summary>
    public required IReadOnlyList<Guid> PlatformAdminRowIds { get; init; }
}
