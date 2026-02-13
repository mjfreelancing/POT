using Pot.Shared.Enumerations;

namespace Pot.App.Features.Incomes.Renew.Models;

public sealed class Input
{
    public required Guid[] RowIds { get; init; } = [];
    public required DateOnly AsOfDate { get; init; }
    public required RenewalMode Mode { get; init; }
}
