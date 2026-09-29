using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Maintenance.Export.Accounts;

/// <summary>
/// Exports all accounts as CSV content for a maintenance export package.
/// </summary>
public interface IAccountsExporter : IPotScopedDependency
{
    /// <summary>
    /// Exports all accounts.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The exported account rows as CSV bytes.</returns>
    Task<byte[]> ExportAllAsync(CancellationToken cancellationToken);
}
