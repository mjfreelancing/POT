namespace Pot.App.Features.Projections.Models;

public sealed class AccountDailyBalances
{
    public required Guid RowId { get; init; }
    public required string Description { get; init; }
    public required List<DateBalance> Dates { get; init; }
}
