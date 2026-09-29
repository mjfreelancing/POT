using AllOverIt.Patterns.Result;
using Pot.App.Features.Maintenance.Import.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Maintenance.Import.Accounts;

/// <summary>
/// Imports account rows from a maintenance export, creating or updating each account in the current site.
/// </summary>
/// <remarks>
/// Rows are matched to existing accounts by <c>RowId</c>: a matching account is updated and any other row is created.
/// Rows are not validated because the data is expected to have been produced by a previous export.
/// </remarks>
public interface IAccountsImporter : IPotScopedDependency
{
    /// <summary>
    /// Imports the supplied account rows.
    /// </summary>
    /// <param name="csvRows">The account rows to import.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <c>EnrichedResult&lt;int&gt;</c> containing the number of rows processed, or a failure from the create or
    /// update operation that rejected a row.
    /// </returns>
    Task<EnrichedResult<int>> ImportAsync(IEnumerable<IAccountCsvRow> csvRows, CancellationToken cancellationToken);
}
