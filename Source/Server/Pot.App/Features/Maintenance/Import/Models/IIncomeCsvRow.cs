using Pot.Shared.Enumerations;

namespace Pot.App.Features.Maintenance.Import.Models;

/// <summary>
/// Represents a single income row read from a maintenance import CSV file.
/// </summary>
/// <remarks>
/// The contract decouples import processing from the CsvHelper-bound row type, so importer logic can be exercised
/// against any implementation of the row shape.
/// </remarks>
public interface IIncomeCsvRow
{
    /// <summary>The row identifier of the account the income belongs to.</summary>
    Guid AccountRowId { get; }

    /// <summary>The income amount.</summary>
    double Amount { get; }

    /// <summary>The income description.</summary>
    string Description { get; }

    /// <summary>The date the income stops recurring, or <see langword="null"/> when it has no end.</summary>
    DateOnly? EndDate { get; }

    /// <summary>Whether the income is excluded from projections.</summary>
    bool ExcludeFromCalcs { get; }

    /// <summary>How often the income recurs.</summary>
    Frequency Frequency { get; }

    /// <summary>The number of frequency units between recurrences.</summary>
    int FrequencyCount { get; }

    /// <summary>The next date the income is due.</summary>
    DateOnly NextDue { get; }

    /// <summary>An optional note about the income.</summary>
    string Note { get; }

    /// <summary>The income's row identifier.</summary>
    Guid RowId { get; }
}