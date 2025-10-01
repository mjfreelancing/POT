using AllOverIt.Patterns.Result;
using Pot.App.Features.Expenses.ToggleExclude.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Expenses.ToggleExclude;

public interface IExcludeExpensesService : IPotScopedDependency
{
    Task<EnrichedResult<bool>> ToggleExclusionAsync(Input input, CancellationToken cancellationToken);
}
