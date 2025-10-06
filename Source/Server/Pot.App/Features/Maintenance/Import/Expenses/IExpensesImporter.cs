using Pot.App.Features.Maintenance.Import.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Maintenance.Import.Expenses;

public interface IExpensesImporter : IPotScopedDependency
{
    Task<int> ImportAsync(IEnumerable<IExpenseCsvRow> csvRows, CancellationToken cancellationToken);
}
