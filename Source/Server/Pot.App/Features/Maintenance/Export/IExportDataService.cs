using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Maintenance.Export;

public interface IExportDataService : IPotScopedDependency
{
    Task<byte[]> ExportAllAsync(string publicKey, CancellationToken cancellationToken);
}
