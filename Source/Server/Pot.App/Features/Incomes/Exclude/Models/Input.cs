namespace Pot.App.Features.Incomes.Exclude.Models;

public sealed class Input
{
    public required Guid[] RowIds { get; init; } = [];
}
