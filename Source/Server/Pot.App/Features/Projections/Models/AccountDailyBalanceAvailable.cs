namespace Pot.App.Features.Projections.Models;

public sealed class AccountDailyBalanceAvailable
{
    public required Guid RowId { get; init; }
    public required string Description { get; init; }
    public required List<DateBalanceAvailable> Dates { get; init; }
}
