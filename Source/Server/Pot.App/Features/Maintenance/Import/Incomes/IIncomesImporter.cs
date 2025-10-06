using Pot.App.Features.Maintenance.Import.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Maintenance.Import.Incomes;

public interface IIncomesImporter : IPotScopedDependency
{
    Task<int> ImportAsync(IEnumerable<IIncomeCsvRow> csvRows, CancellationToken cancellationToken);
}
