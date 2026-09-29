using Pot.App.Features.Users.UpdateStatus.Models;

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
    /// Gets the concurrency eTag of the stored user whose status is changing.
    /// </summary>
    public required long UserEtag { get; init; }
}
