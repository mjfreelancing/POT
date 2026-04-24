using Pot.App.Concerns.Accruals.Models;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Concerns.Accruals;

public interface IAccountAccrualDirtyMarker : IPotScopedDependency
{
    // Returns account Ids that should be marked dirty based on the change in accrual state. This is used to determine which accounts need to have their accruals recalculated.
    int[] GetAccountIdsToMarkDirty(ExpenseAccrualState before, ExpenseAccrualState after);

    // Returns true if the deletion of the expense should mark the account as dirty. This is used to determine if an account needs to have its accruals recalculated when an expense is deleted.
    bool IsDirtyImpactingDelete(ExpenseEntity expense, DateOnly asOfDate);

    // Marks the account as dirty for accrual recalculation.
    Task MarkDirtyForAccountAsync(AccountEntity account, CancellationToken cancellationToken);

    // Clears account dirty state and stamps the last accrued date after successful accrual completion.
    Task ClearDirtyOnAccrualSuccessAsync(AccountEntity account, DateOnly asOfDate, CancellationToken cancellationToken);

    // Marks the accounts as dirty for accrual recalculation based on the expenses.
    Task MarkDirtyForExpensesAsync(IReadOnlyCollection<ExpenseEntity> expenses, CancellationToken cancellationToken);
}
