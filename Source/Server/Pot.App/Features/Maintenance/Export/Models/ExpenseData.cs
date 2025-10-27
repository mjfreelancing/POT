using Pot.Shared.Enumerations;

namespace Pot.App.Features.Maintenance.Export.Models;

public sealed class ExpenseData
{
    public required Guid RowId { get; set; }
    public bool ExcludeFromCalcs { get; set; }
    public required string Description { get; set; }
    public required DateOnly AccrualStart { get; set; }
    public required DateOnly NextDue { get; set; }
    public DateOnly? EndDate { get; set; }
    public required Frequency Frequency { get; set; }
    public required int FrequencyCount { get; set; }
    public required double Amount { get; set; }
    public required double Accrued { get; set; }
    public required bool AccruedIsDirty { get; set; }
    public DateOnly? LastAccruedUpdate { get; init; }
    public string? Note { get; set; }
    public required Guid AccountRowId { get; set; }
}
