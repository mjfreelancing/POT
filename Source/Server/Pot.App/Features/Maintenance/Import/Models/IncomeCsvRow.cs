using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using Pot.App.Concerns.Csv;
using Pot.Shared.Enumerations;

namespace Pot.App.Features.Maintenance.Import.Models;

/// <summary>
/// Default implementation of <see cref="IIncomeCsvRow"/>.
/// </summary>
/// <remarks>
/// The <c>Index</c> attributes fix the CSV column order the importer expects, and the configured converters
/// define how each column's text is parsed.
/// </remarks>
internal sealed class IncomeCsvRow : IIncomeCsvRow
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
    [TypeConverter(typeof(DateOnlyConverter))]
    public DateOnly NextDue { get; init; }

    /// <inheritdoc />
    [Index(4)]
    [Format("yyyy-MM-dd")]
    [TypeConverter(typeof(NullableDateOnlyConverter))]
    public DateOnly? EndDate { get; init; }

    /// <inheritdoc />
    [Index(5)]
    [TypeConverter(typeof(FrequencyConverter))]
    public required Frequency Frequency { get; init; }

    /// <inheritdoc />
    [Index(6)]
    public int FrequencyCount { get; init; }

    /// <inheritdoc />
    [Index(7)]
    public double Amount { get; init; }

    /// <inheritdoc />
    [Index(8)]
    public string Note { get; init; } = string.Empty;

    /// <inheritdoc />
    [Index(9)]
    public Guid AccountRowId { get; init; }
}
