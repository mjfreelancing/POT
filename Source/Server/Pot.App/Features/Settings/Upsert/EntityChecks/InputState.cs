using Pot.App.Features.Settings.Upsert.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Settings.Upsert.EntityChecks;

/// <summary>
/// Carries the state evaluated by the pre-update checks during a settings upsert.
/// </summary>
internal sealed class InputState
{
    /// <summary>
    /// Gets the requested setting change.
    /// </summary>
    public required Input Input { get; init; }

    /// <summary>
    /// Gets the stored setting, or <see langword="null" /> when the setting does not exist yet.
    /// </summary>
    public required SettingEntity? SettingToUpdate { get; init; }
}
