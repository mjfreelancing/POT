using Pot.App.Features.Approvals.UpdateStatus.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Approvals.UpdateStatus.EntityChecks;

/// <summary>
/// Carries the approval request and the user being validated through the pre-update check chain.
/// </summary>
internal sealed class InputState
{
    public required Input Input { get; init; }
    public required UserEntity UserToUpdate { get; init; }
}
