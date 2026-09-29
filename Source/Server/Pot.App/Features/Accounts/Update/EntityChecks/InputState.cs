using Pot.App.Features.Accounts.Update.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Accounts.Update.EntityChecks;

/// <summary>
/// Carries the update request and the account being validated through the pre-update check chain.
/// </summary>
internal sealed class InputState
{
    public required Input Input { get; init; }
    public required AccountEntity AccountToUpdate { get; init; }
}
