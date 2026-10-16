namespace Pot.App.Features.Expenses.ToggleExclude.Models;

public sealed class Input
{
    public required IReadOnlyList<Guid> RowIds { get; init; } = [];
}
