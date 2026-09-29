using CsvHelper.Configuration.Attributes;

namespace Pot.App.Features.Maintenance.Import.Models;

/// <summary>
/// Default implementation of <see cref="IAccountCsvRow"/>.
/// </summary>
/// <remarks>
/// The <c>Index</c> attributes fix the CSV column order the importer expects.
/// </remarks>
internal sealed class AccountCsvRow : IAccountCsvRow
{
    /// <inheritdoc />
    [Index(0)]
    public Guid RowId { get; init; }

    /// <inheritdoc />
    [Index(1)]
    public string Description { get; init; } = string.Empty;

    /// <inheritdoc />
    [Index(2)]
    public double Balance { get; init; }

    /// <inheritdoc />
    [Index(3)]
    public double Reserved { get; init; }
}
