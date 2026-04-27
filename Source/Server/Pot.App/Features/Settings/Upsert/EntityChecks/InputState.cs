using Pot.App.Features.Settings.Upsert.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Settings.Upsert.EntityChecks;

internal sealed class InputState
{
    public required Input Input { get; init; }
    public required SettingEntity? SettingToUpdate { get; init; }
}
