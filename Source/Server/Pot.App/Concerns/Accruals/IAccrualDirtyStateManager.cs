using Pot.App.Concerns.Accruals.Models;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Concerns.Accruals;

public interface IAccrualDirtyStateManager : IPotScopedDependency
{
    // Returns account Ids that should be marked dirty based on the change in accrual state.
    int[] GetAccountsRequiringRecalc(ExpenseAccrualState before, ExpenseAccrualState after, DateOnly asOfDate);

    // Returns true when deleting the expense impacts account accrual state.
    bool IsExpenseDeletionImpactful(ExpenseEntity expense, DateOnly asOfDate);

    // Marks account accrual rows dirty for the provided account ids.
    Task SetAccountsDirtyAsync(IReadOnlyCollection<int> accountIds, CancellationToken cancellationToken);

    // Marks account accrual rows dirty for the distinct accounts associated with the expenses.
    Task SetAccountsDirtyAsync(IReadOnlyCollection<ExpenseEntity> expenses, CancellationToken cancellationToken);

    // Clears dirty state and stamps the last accrued date for the account.
    Task SetAccountCleanAsync(int accountId, DateOnly asOfDate, CancellationToken cancellationToken);
}
