using Pot.App.Features.Approvals.UpdateStatus.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Approvals.UpdateStatus.EntityChecks;

internal sealed class InputState
{
    public required Input Input { get; init; }
    public required UserEntity UserToUpdate { get; init; }
}
