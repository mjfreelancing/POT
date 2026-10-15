namespace Pot.App.Features.Incomes.ToggleExclude.Models;

public sealed class Input
{
    public required Guid[] RowIds { get; init; } = [];
}
