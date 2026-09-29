using Pot.Data.Entities;

namespace Pot.App.Features.Accounts.Create.EntityChecks;

/// <summary>
/// Carries the account being validated through the pre-create check chain.
/// </summary>
internal sealed class InputState
{
    public required AccountEntity AccountToCreate { get; init; }
}
