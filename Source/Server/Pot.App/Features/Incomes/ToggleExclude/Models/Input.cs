namespace Pot.App.Features.Incomes.ToggleExclude.Models;

public sealed class Input
{
    public required IReadOnlyList<Guid> RowIds { get; init; } = [];
}
