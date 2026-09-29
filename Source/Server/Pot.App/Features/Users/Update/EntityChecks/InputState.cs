using Pot.App.Features.Users.Update.Models;

namespace Pot.App.Features.Users.Update.EntityChecks;

/// <summary>
/// Carries the state evaluated by the pre-update checks during a user update.
/// </summary>
internal sealed class InputState
{
    /// <summary>
    /// Gets the requested user change.
    /// </summary>
    public required Input Input { get; init; }

    /// <summary>
    /// Gets the concurrency eTag of the stored user being updated.
    /// </summary>
    public required long UserEtag { get; init; }
}
