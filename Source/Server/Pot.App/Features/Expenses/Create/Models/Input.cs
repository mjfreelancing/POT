using Pot.Shared.Enumerations;

namespace Pot.App.Features.Expenses.Create.Models;

public sealed class Input
{
    // Provided when importing
    public Guid? RowId { get; init; }

    public bool ExcludeFromCalcs { get; init; }
    public string Description { get; init; } = string.Empty;
    public DateOnly? AccrualStart { get; init; }
    public DateOnly NextDue { get; init; }
    public DateOnly? EndDate { get; init; }
    public required AccrualPolicy AccrualPolicy { get; init; }
    public required Frequency Frequency { get; init; }
    public int FrequencyCount { get; init; }
    public double Amount { get; init; }
    public string? Note { get; init; }
    public Guid AccountRowId { get; init; }
}
