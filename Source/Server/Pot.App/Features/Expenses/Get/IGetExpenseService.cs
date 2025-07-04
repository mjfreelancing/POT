using AllOverIt.Patterns.Result;
using Pot.App.Features.Expenses.Get.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Expenses.Get;

public interface IGetExpenseService : IPotScopedDependency
{
    Task<EnrichedResult<Output>> GetExpenseAsync(Guid expenseId, CancellationToken cancellationToken);
}
