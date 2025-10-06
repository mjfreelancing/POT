using CsvHelper.Configuration.Attributes;

namespace Pot.App.Features.Maintenance.Import.Models;

internal sealed class AccountCsvRow : IAccountCsvRow
{
    [Index(0)]
    public Guid RowId { get; init; }

    [Index(1)]
    public string Bsb { get; init; } = string.Empty;

    [Index(2)]
    public string Number { get; init; } = string.Empty;

    [Index(3)]
    public string Description { get; init; } = string.Empty;

    [Index(4)]
    public double Balance { get; init; }

    [Index(5)]
    public double Reserved { get; init; }

    [Index(6)]
    public double TotalExpenseAccrued { get; init; }

    [Index(7)]
    public double DailyExpenseAccrual { get; init; }
}
