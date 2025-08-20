namespace Pot.App.Features.Expenses.Exclude.Models;

public sealed class Input
{
    public required Guid[] RowIds { get; init; } = [];
    public bool ExcludeFromCalcs { get; init; }
}
