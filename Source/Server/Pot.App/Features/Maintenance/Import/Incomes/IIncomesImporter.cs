using AllOverIt.Patterns.Result;
using Pot.App.Features.Maintenance.Import.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Maintenance.Import.Incomes;

/// <summary>
/// Imports income rows from a maintenance export, creating or updating each income in the current site.
/// </summary>
/// <remarks>
/// Each row's account is resolved from its <c>AccountRowId</c> and must already exist, so accounts are expected to be
/// imported first. Rows are matched to existing incomes by <c>RowId</c> and are not validated because the data is
/// expected to have been produced by a previous export.
/// </remarks>
public interface IIncomesImporter : IPotScopedDependency
{
    /// <summary>
    /// Imports the supplied income rows.
    /// </summary>
    /// <param name="csvRows">The income rows to import.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <c>EnrichedResult&lt;int&gt;</c> containing the number of rows processed, or a failure from the create or
    /// update operation that rejected a row.
    /// </returns>
    Task<EnrichedResult<int>> ImportAsync(IEnumerable<IIncomeCsvRow> csvRows, CancellationToken cancellationToken);
}
