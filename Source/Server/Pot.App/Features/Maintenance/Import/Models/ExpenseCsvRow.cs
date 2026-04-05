using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using Pot.App.Concerns.Csv;
using Pot.Shared.Enumerations;

namespace Pot.App.Features.Maintenance.Import.Models;

internal sealed class ExpenseCsvRow : IExpenseCsvRow
{
    [Index(0)]
    public Guid RowId { get; init; }

    [Index(1)]
    public bool ExcludeFromCalcs { get; init; }

    [Index(2)]
    public string Description { get; init; } = string.Empty;

    [Index(3)]
    [Format("yyyy-MM-dd")]
    [TypeConverter(typeof(NullableDateOnlyConverter))]
    public DateOnly? AccrualStart { get; init; }

    [Index(4)]
    [Format("yyyy-MM-dd")]
    [TypeConverter(typeof(DateOnlyConverter))]
    public DateOnly NextDue { get; init; }

    [Index(5)]
    [Format("yyyy-MM-dd")]
    [TypeConverter(typeof(NullableDateOnlyConverter))]
    public DateOnly? EndDate { get; init; }

    [Index(6)]
    [TypeConverter(typeof(AccrualPolicyConverter))]
    public required AccrualPolicy AccrualPolicy { get; init; }

    [Index(7)]
    [TypeConverter(typeof(FrequencyConverter))]
    public required Frequency Frequency { get; init; }

    [Index(8)]
    public int FrequencyCount { get; init; }

    [Index(9)]
    public double Amount { get; init; }

    [Index(10)]
    public double Accrued { get; init; }

    [Index(11)]
    public bool AccruedIsDirty { get; init; }

    [Index(12)]
    [Format("yyyy-MM-dd")]
    [TypeConverter(typeof(NullableDateOnlyConverter))]
    public DateOnly? LastAccruedUpdate { get; init; }

    [Index(13)]
    public string Note { get; init; } = string.Empty;

    [Index(14)]
    public Guid AccountRowId { get; init; }
}
