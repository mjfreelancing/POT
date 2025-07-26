using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using Pot.App.Concerns.Csv;
using Pot.Shared;

namespace Pot.App.Features.Maintenance.Import.Incomes.Models;

internal sealed class IncomeCsvRow
{
    [Index(0)]
    public Guid RowId { get; init; }

    [Index(1)]
    public string Description { get; init; } = string.Empty;

    [Index(2)]
    [Format("yyyy-MM-dd")]
    [TypeConverter(typeof(DateOnlyConverter))]
    public DateOnly NextDue { get; init; }

    [Index(3)]
    [Format("yyyy-MM-dd")]
    [TypeConverter(typeof(NullableDateOnlyConverter))]
    public DateOnly? EndDate { get; init; }

    [Index(4)]
    [TypeConverter(typeof(FrequencyConverter))]
    public required Frequency Frequency { get; init; }

    [Index(5)]
    public int FrequencyCount { get; init; }

    [Index(6)]
    public double Amount { get; init; }

    [Index(7)]
    public string Note { get; init; } = string.Empty;

    [Index(8)]
    public Guid AccountRowId { get; init; }
}
