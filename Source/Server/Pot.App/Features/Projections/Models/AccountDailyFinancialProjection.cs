namespace Pot.App.Features.Projections.Models;

public sealed class AccountDailyFinancialProjection
{
    public required Guid RowId { get; init; }
    public required string Description { get; init; }
    public required List<DateProjection> Dates { get; init; }
}
