using AllOverIt.Patterns.Result;
using Pot.App.Concerns.DependencyInjection;

namespace Pot.App.Features.Expenses.Delete;

public interface IDeleteExpenseService : IPotScopedDependency
{
    Task<EnrichedResult<bool>> DeleteExpenseAsync(Guid expenseId, CancellationToken cancellationToken);
}
