using AllOverIt.Patterns.Result;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Maintenance.Import;

/// <summary>
/// Imports a maintenance export package, creating or updating the accounts, expenses, and incomes it contains.
/// </summary>
/// <remarks>
/// The supplied stream is owned by the import and is disposed when the operation completes. Only the current package
/// version is accepted; a package written with any other metadata version is rejected rather than migrated. Accounts
/// are imported before expenses and incomes because those records reference accounts.
/// </remarks>
public interface IImportDataService : IPotScopedDependency
{
    /// <summary>
    /// Imports the contents of a maintenance export package.
    /// </summary>
    /// <param name="zipStream">The stream containing the export package to import.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <c>EnrichedResult&lt;int&gt;</c> containing the total number of rows imported across accounts, expenses, and
    /// incomes, or a failure describing why the package was rejected or the import could not complete.
    /// </returns>
    Task<EnrichedResult<int>> ImportAsync(Stream zipStream, CancellationToken cancellationToken);
}
