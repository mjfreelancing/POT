using Pot.Shared;

namespace Pot.App.Features.Expenses.Update.Models;

public sealed class Input
{
    public Guid RowId { get; init; }
    public long Etag { get; init; }
    public string Description { get; init; } = string.Empty;
    public DateOnly AccrualStart { get; init; }
    public DateOnly NextDue { get; init; }
    public DateOnly? EndDate { get; init; }
    public Frequency Frequency { get; init; } = Frequency.Months;
    public int FrequencyCount { get; init; }
    public double Amount { get; init; }
    public bool Recurring { get; init; }
    public Guid AccountRowId { get; init; }
}
