namespace Pot.App.Features.Accruals.Status.Models;

public sealed class Input
{
    public IReadOnlyList<Guid> AccountRowIds { get; init; } = [];
    public DateOnly AsOfDate { get; init; }
}