using Pot.Shared.Enumerations;

namespace Pot.App.Features.Expenses.Update.Models;

public sealed class Input
{
    public Guid RowId { get; init; }
    public long Etag { get; init; }
    public bool ExcludeFromCalcs { get; init; }
    public required string Description { get; init; }
    public DateOnly? AccrualStart { get; init; }
    public DateOnly NextDue { get; init; }
    public DateOnly? EndDate { get; init; }
    public required Frequency Frequency { get; init; }
    public int FrequencyCount { get; init; }
    public double Amount { get; init; }
    public string? Note { get; init; }
    public Guid AccountRowId { get; init; }
}
