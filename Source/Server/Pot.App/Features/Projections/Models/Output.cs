namespace Pot.App.Features.Projections.Models;

public sealed class Output
{
    public required List<AccountDailyBalanceAvailable> Accounts { get; init; }
    public required List<DateBalanceAvailable> Global { get; init; }
}
