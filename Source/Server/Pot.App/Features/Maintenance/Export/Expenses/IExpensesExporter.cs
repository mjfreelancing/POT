using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Maintenance.Export.Expenses;

/// <summary>
/// Exports all expenses as CSV content for a maintenance export package.
/// </summary>
public interface IExpensesExporter : IPotScopedDependency
{
    /// <summary>
    /// Exports all expenses.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The exported expense rows as CSV bytes.</returns>
    Task<byte[]> ExportAllAsync(CancellationToken cancellationToken);
}
