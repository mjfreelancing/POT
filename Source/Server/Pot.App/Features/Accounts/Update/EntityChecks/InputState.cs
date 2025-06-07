using Pot.App.Features.Accounts.Update.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Accounts.Update.EntityChecks;

internal sealed class InputState
{
    public required Input Input { get; init; }
    public required AccountEntity AccountToUpdate { get; init; }
}
