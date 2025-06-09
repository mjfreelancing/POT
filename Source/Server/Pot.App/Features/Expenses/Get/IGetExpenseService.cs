using Pot.App.Concerns.DependencyInjection;
using Pot.App.Features.Expenses.Get.Models;

namespace Pot.App.Features.Expenses.Get;

public interface IGetExpenseService : IPotScopedDependency
{
    Task<Output?> GetExpenseAsync(Guid expenseId, CancellationToken cancellationToken);
}
