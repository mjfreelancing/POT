using AllOverIt.Patterns.Result;
using Pot.App.Features.Accounts.AccrueExpenses.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accounts.AccrueExpenses;

public interface IAccrueExpensesService : IPotScopedDependency
{
    Task<EnrichedResult<bool>> AccrueAsync(Input input, CancellationToken cancellationToken);
}
