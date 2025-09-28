namespace Pot.App.Features.Accruals.Status.Models;

public sealed class Input
{
    // Account RowId's
    public Guid[] RowIds { get; init; } = [];
    public DateOnly beforeDate { get; init; }
}