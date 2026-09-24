using CsvHelper.Configuration.Attributes;

namespace Pot.App.Features.Maintenance.Import.Models;

internal sealed class AccountCsvRow : IAccountCsvRow
{
    [Index(0)]
    public Guid RowId { get; init; }

    [Index(1)]
    public string Description { get; init; } = string.Empty;

    [Index(2)]
    public double Balance { get; init; }

    [Index(3)]
    public double Reserved { get; init; }

    [Index(4)]
    public double TotalExpenseAccrued { get; init; }

    [Index(5)]
    public double DailyExpenseAccrual { get; init; }
}
