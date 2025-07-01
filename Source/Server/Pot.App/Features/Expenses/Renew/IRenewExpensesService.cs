using AllOverIt.Patterns.Result;
using Pot.App.Features.Expenses.Renew.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Expenses.Renew;

public interface IRenewExpensesService : IPotScopedDependency
{
    Task<EnrichedResult<bool>> RenewAsync(Input input, CancellationToken cancellationToken);
}
