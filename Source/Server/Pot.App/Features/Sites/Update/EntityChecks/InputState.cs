using Pot.App.Features.Sites.Update.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Sites.Update.EntityChecks;

/// <summary>
/// Carries the state evaluated by the pre-update checks during a site update.
/// </summary>
internal sealed class InputState
{
    /// <summary>
    /// Gets the requested site change.
    /// </summary>
    public required Input Input { get; init; }

    /// <summary>
    /// Gets the stored site being updated.
    /// </summary>
    public required SiteEntity SiteToUpdate { get; init; }
}
