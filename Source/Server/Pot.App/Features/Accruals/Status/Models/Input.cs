namespace Pot.App.Features.Accruals.Status.Models;

public sealed class Input
{
    public Guid[] AccountRowIds { get; init; } = [];
    public DateOnly BeforeDate { get; init; }
}