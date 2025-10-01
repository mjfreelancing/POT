namespace Pot.App.Features.Expenses.ToggleExclude.Models;

public sealed class Input
{
    public required Guid[] RowIds { get; init; } = [];
}
