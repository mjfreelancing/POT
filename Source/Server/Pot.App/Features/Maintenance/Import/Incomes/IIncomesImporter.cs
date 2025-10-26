using AllOverIt.Patterns.Result;
using Pot.App.Features.Maintenance.Import.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Maintenance.Import.Incomes;

public interface IIncomesImporter : IPotScopedDependency
{
    Task<EnrichedResult<int>> ImportAsync(IEnumerable<IIncomeCsvRow> csvRows, CancellationToken cancellationToken);
}
