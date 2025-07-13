using Pot.Shared;

namespace Pot.App.Features.Incomes.Create.Models;

public sealed class Input
{
    public string Description { get; init; } = string.Empty;
    public DateOnly NextDue { get; init; }
    public DateOnly? EndDate { get; init; }
    public Frequency Frequency { get; init; } = Frequency.Months;
    public int FrequencyCount { get; init; }
    public double Amount { get; init; }
    public string? Note { get; init; }
    public Guid AccountRowId { get; init; }
}
