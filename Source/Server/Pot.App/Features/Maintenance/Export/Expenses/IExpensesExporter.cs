using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Maintenance.Export.Expenses;

public interface IExpensesExporter : IPotScopedDependency
{
    Task<byte[]> ExportAllAsync(CancellationToken cancellationToken);
}
