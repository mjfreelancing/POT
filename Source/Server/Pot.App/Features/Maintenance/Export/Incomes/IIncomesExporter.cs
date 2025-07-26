using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Maintenance.Export.Incomes;

public interface IIncomesExporter : IPotScopedDependency
{
    Task<byte[]> ExportAllAsync(CancellationToken cancellationToken);
}
