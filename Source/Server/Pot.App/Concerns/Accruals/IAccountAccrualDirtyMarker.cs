using Pot.App.Concerns.Accruals.Models;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Concerns.Accruals;

public interface IAccountAccrualDirtyMarker : IPotScopedDependency
{
    int[] GetAccountIdsToMarkDirty(ExpenseAccrualState before, ExpenseAccrualState after);
    Task MarkDirtyForAccountAsync(AccountEntity account, CancellationToken cancellationToken);
    Task MarkDirtyForExpensesAsync(IReadOnlyCollection<ExpenseEntity> expenses, CancellationToken cancellationToken);
}
