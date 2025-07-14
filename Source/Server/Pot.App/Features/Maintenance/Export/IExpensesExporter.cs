using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Maintenance.Export;

public interface IExpensesExporter : IPotScopedDependency
{
    Task<byte[]> ExportAllAsync(CancellationToken cancellationToken);
}
