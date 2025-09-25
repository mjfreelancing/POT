using Pot.App.Features.Users.Update.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Users.Update.EntityChecks;

internal sealed class InputState
{
    public required Input Input { get; init; }
    public required UserEntity UserToUpdate { get; init; }
}
