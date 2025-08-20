using AllOverIt.Patterns.Result;
using Pot.App.Features.Incomes.Exclude.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Incomes.Exclude;

public interface IExcludeIncomesService : IPotScopedDependency
{
    Task<EnrichedResult<bool>> ExcludeAsync(Input input, CancellationToken cancellationToken);
}
