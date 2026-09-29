namespace Pot.App.Features.Accounts.Delete.EntityChecks;

/// <summary>
/// Carries the identifier of the account being validated through the pre-delete check chain.
/// </summary>
internal sealed class InputState
{
    public required Guid AccountId { get; init; }
}
