namespace Pot.App.Features.Projections.Models;

public sealed class ProjectionExpenseModel
{
    public required Guid RowId { get; init; }
    public required string Description { get; init; }
    public required double Amount { get; init; }
}
