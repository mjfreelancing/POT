namespace Pot.App.Features.Projections.Models;

public sealed class Output
{
    public required List<AccountDailyProjection> Accounts { get; init; }
    public required List<DateProjection> Global { get; init; }
}
