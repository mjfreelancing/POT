using Pot.App.Features.Expenses.Get.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Expenses.Get;

public interface IGetExpenseService : IPotScopedDependency
{
    Task<Output?> GetExpenseAsync(Guid expenseId, CancellationToken cancellationToken);
}
