using AllOverIt.Patterns.Result;
using Pot.App.Features.Maintenance.Import.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Maintenance.Import.Expenses;

/// <summary>
/// Imports expense rows from a maintenance export, creating or updating each expense in the current site.
/// </summary>
/// <remarks>
/// Each row's account is resolved from its <c>AccountRowId</c> and must already exist, so accounts are expected to be
/// imported first. Rows are matched to existing expenses by <c>RowId</c> and are not validated because the data is
/// expected to have been produced by a previous export.
/// </remarks>
public interface IExpensesImporter : IPotScopedDependency
{
    /// <summary>
    /// Imports the supplied expense rows.
    /// </summary>
    /// <param name="csvRows">The expense rows to import.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <c>EnrichedResult&lt;int&gt;</c> containing the number of rows processed, or a failure from the create or
    /// update operation that rejected a row.
    /// </returns>
    Task<EnrichedResult<int>> ImportAsync(IEnumerable<IExpenseCsvRow> csvRows, CancellationToken cancellationToken);
}
