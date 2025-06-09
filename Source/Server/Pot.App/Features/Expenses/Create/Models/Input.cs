using Pot.Shared;

namespace Pot.App.Features.Expenses.Create.Models;

public sealed class Input
{
    public string Description { get; init; } = string.Empty;
    public DateOnly NextDue { get; init; }
    public DateOnly AccrualStart { get; init; }
    public DateOnly? EndDate { get; init; }
    public Frequency Frequency { get; init; } = Frequency.Months;
    public int FrequencyCount { get; init; }
    public double Amount { get; init; }
    public bool Recurring { get; init; }
    public Guid AccountRowId { get; init; }
}
