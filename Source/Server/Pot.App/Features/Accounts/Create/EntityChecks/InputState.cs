using Pot.Data.Entities;

namespace Pot.App.Features.Accounts.Create.EntityChecks;

internal sealed class InputState
{
    public required AccountEntity AccountToCreate { get; init; }
}
