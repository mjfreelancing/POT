namespace Pot.App.Features.Projections.Models;

public sealed class Output
{
    public required AccountsDailyBalances Accounts { get; init; }
    public required GlobalDailyBalances Global { get; init; }
}
