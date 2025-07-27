using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Maintenance.Import.Expenses;

public interface IExpensesImporter : IPotScopedDependency
{
    Task<int> ImportAsync(Stream dataStream, CancellationToken cancellationToken);
}
