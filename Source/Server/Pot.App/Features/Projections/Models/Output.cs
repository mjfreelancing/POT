namespace Pot.App.Features.Projections.Models;

public sealed class Output
{
    public required List<AccountDailyFinancialProjection> Accounts { get; init; }
    public required List<DateProjection> Global { get; init; }
}
