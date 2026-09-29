using Pot.Shared.Enumerations;

namespace Pot.App.Features.Maintenance.Import.Models;

/// <summary>
/// Represents a single expense row read from a maintenance import CSV file.
/// </summary>
/// <remarks>
/// The contract decouples import processing from the CsvHelper-bound row type, so importer logic can be exercised
/// against any implementation of the row shape.
/// </remarks>
public interface IExpenseCsvRow
{
    /// <summary>The row identifier of the account the expense belongs to.</summary>
    Guid AccountRowId { get; }

    /// <summary>The accrual policy applied to the expense.</summary>
    AccrualPolicy AccrualPolicy { get; }

    /// <summary>The date accrual starts, or <see langword="null"/> when accrual starts from the first due date.</summary>
    DateOnly? AccrualStart { get; }

    /// <summary>The expense amount.</summary>
    double Amount { get; }

    /// <summary>The expense description.</summary>
    string Description { get; }

    /// <summary>The date the expense stops recurring, or <see langword="null"/> when it has no end.</summary>
    DateOnly? EndDate { get; }

    /// <summary>Whether the expense is excluded from projections.</summary>
    bool ExcludeFromCalcs { get; }

    /// <summary>How often the expense recurs.</summary>
    Frequency Frequency { get; }

    /// <summary>The number of frequency units between recurrences.</summary>
    int FrequencyCount { get; }

    /// <summary>The next date the expense is due.</summary>
    DateOnly NextDue { get; }

    /// <summary>An optional note about the expense.</summary>
    string Note { get; }

    /// <summary>The expense's row identifier.</summary>
    Guid RowId { get; }
}