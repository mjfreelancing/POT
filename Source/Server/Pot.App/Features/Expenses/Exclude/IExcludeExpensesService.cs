using AllOverIt.Patterns.Result;
using Pot.App.Features.Expenses.Exclude.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Expenses.Exclude;

public interface IExcludeExpensesService : IPotScopedDependency
{
    Task<EnrichedResult<bool>> ExcludeAsync(Input input, CancellationToken cancellationToken);
}
