using AllOverIt.Patterns.Result;
using Pot.App.Features.Accruals.AccrueExpenses.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accruals.AccrueExpenses;

public interface IAccrueExpensesService : IPotScopedDependency
{
    Task<EnrichedResult<bool>> AccrueAsync(Input input, CancellationToken cancellationToken);
}
