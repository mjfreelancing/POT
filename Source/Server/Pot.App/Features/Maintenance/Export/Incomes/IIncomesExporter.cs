using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Maintenance.Export.Incomes;

/// <summary>
/// Exports all incomes as CSV content for a maintenance export package.
/// </summary>
public interface IIncomesExporter : IPotScopedDependency
{
    /// <summary>
    /// Exports all incomes.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The exported income rows as CSV bytes.</returns>
    Task<byte[]> ExportAllAsync(CancellationToken cancellationToken);
}
