using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Maintenance.Export;

/// <summary>
/// Exports all accounts, incomes, and expenses into a single maintenance export package.
/// </summary>
/// <remarks>
/// The package contains <c>metadata</c>, <c>accounts</c>, <c>incomes</c>, and <c>expenses</c> entries, written in the
/// current package version.
/// </remarks>
public interface IExportDataService : IPotScopedDependency
{
    /// <summary>
    /// Exports all accounts, incomes, and expenses as a maintenance export package.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The bytes of the completed export package.</returns>
    Task<byte[]> ExportAllAsync(CancellationToken cancellationToken);
}
