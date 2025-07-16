namespace Pot.App.Features.Incomes.Renew.Models;

public sealed class Input
{
    public required Guid[] RowIds { get; init; } = [];
    public required DateOnly UntilDate { get; init; }
}
