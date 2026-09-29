using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using Pot.App.Concerns.Csv;
using Pot.Shared.Enumerations;

namespace Pot.App.Features.Maintenance.Import.Models;

/// <summary>
/// Default implementation of <see cref="IExpenseCsvRow"/>.
/// </summary>
/// <remarks>
/// The <c>Index</c> attributes fix the CSV column order the importer expects, and the configured converters
/// define how each column's text is parsed.
/// </remarks>
internal sealed class ExpenseCsvRow : IExpenseCsvRow
{
    /// <inheritdoc />
    [Index(0)]
    public Guid RowId { get; init; }

    /// <inheritdoc />
    [Index(1)]
    public bool ExcludeFromCalcs { get; init; }

    /// <inheritdoc />
    [Index(2)]
    public string Description { get; init; } = string.Empty;

    /// <inheritdoc />
    [Index(3)]
    [Format("yyyy-MM-dd")]
    [TypeConverter(typeof(NullableDateOnlyConverter))]
    public DateOnly? AccrualStart { get; init; }

    /// <inheritdoc />
    [Index(4)]
    [Format("yyyy-MM-dd")]
    [TypeConverter(typeof(DateOnlyConverter))]
    public DateOnly NextDue { get; init; }

    /// <inheritdoc />
    [Index(5)]
    [Format("yyyy-MM-dd")]
    [TypeConverter(typeof(NullableDateOnlyConverter))]
    public DateOnly? EndDate { get; init; }

    /// <inheritdoc />
    [Index(6)]
    [TypeConverter(typeof(AccrualPolicyConverter))]
    public required AccrualPolicy AccrualPolicy { get; init; }

    /// <inheritdoc />
    [Index(7)]
    [TypeConverter(typeof(FrequencyConverter))]
    public required Frequency Frequency { get; init; }

    /// <inheritdoc />
    [Index(8)]
    public int FrequencyCount { get; init; }

    /// <inheritdoc />
    [Index(9)]
    public double Amount { get; init; }

    /// <inheritdoc />
    [Index(10)]
    public string Note { get; init; } = string.Empty;

    /// <inheritdoc />
    [Index(11)]
    public Guid AccountRowId { get; init; }
}
