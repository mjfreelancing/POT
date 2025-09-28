namespace Pot.App.Features.Accruals.AccrueExpenses.Models;

public sealed class Input
{
    // Account RowId's to accrue expenses
    public Guid[] RowIds { get; init; } = [];
}