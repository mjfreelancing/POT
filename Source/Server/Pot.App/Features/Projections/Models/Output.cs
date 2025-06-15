namespace Pot.App.Features.Projections.Models;

public sealed class Output
{
    public required List<AccountDailyBalances> Accounts { get; init; }
    public required List<DateBalance> Global { get; init; }
}
