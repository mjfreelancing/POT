
namespace Pot.App.Features.Maintenance.Import.Models;

/// <summary>
/// Represents a single account row read from a maintenance import CSV file.
/// </summary>
/// <remarks>
/// The contract decouples import processing from the CsvHelper-bound row type, so importer logic can be exercised
/// against any implementation of the row shape.
/// </remarks>
public interface IAccountCsvRow
{
    /// <summary>The account's starting balance.</summary>
    double Balance { get; }

    /// <summary>The account description.</summary>
    string Description { get; }

    /// <summary>The amount held back from the account balance.</summary>
    double Reserved { get; }

    /// <summary>The account's row identifier, used to link the imported expenses and incomes.</summary>
    Guid RowId { get; }
}