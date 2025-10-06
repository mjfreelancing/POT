using Pot.Shared;

namespace Pot.App.Features.Maintenance.Import.Models;

public interface IExpenseCsvRow
{
    Guid AccountRowId { get; }
    DateOnly AccrualStart { get; }
    double Accrued { get; }
    bool AccruedIsDirty { get; }
    double Amount { get; }
    string Description { get; }
    DateOnly? EndDate { get; }
    bool ExcludeFromCalcs { get; }
    Frequency Frequency { get; }
    int FrequencyCount { get; }
    DateOnly? LastAccruedUpdate { get; }
    DateOnly NextDue { get; }
    string Note { get; }
    Guid RowId { get; }
}