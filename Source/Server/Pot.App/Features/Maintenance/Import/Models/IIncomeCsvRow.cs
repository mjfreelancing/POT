using Pot.Shared;

namespace Pot.App.Features.Maintenance.Import.Models;

public interface IIncomeCsvRow
{
    Guid AccountRowId { get; }
    double Amount { get; }
    string Description { get; }
    DateOnly? EndDate { get; }
    bool ExcludeFromCalcs { get; }
    Frequency Frequency { get; }
    int FrequencyCount { get; }
    DateOnly NextDue { get; }
    string Note { get; }
    Guid RowId { get; }
}