using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pot.App.Concerns.Accruals.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.AccountAccrual;

namespace Pot.App.Concerns.Accruals;

internal sealed class AccountAccrualDirtyMarker : IAccountAccrualDirtyMarker
{
    private readonly IPersistableAccountAccrualRepository _accountAccrualRepository;
    private readonly ILogger<AccountAccrualDirtyMarker> _logger;

    public AccountAccrualDirtyMarker(IPersistableAccountAccrualRepository accountAccrualRepository, ILogger<AccountAccrualDirtyMarker> logger)
    {
        _accountAccrualRepository = accountAccrualRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public int[] GetAccountIdsToMarkDirty(ExpenseAccrualState before, ExpenseAccrualState after)
    {
        _logger.LogCall(this);

        _ = before.WhenNotNull();
        _ = after.WhenNotNull();

        if (before == after)
        {
            return [];
        }

        if (before.AccountId == after.AccountId)
        {
            return [after.AccountId];
        }

        return [before.AccountId, after.AccountId];
    }

    public bool IsDirtyImpactingDelete(ExpenseEntity expense, DateOnly asOfDate)
    {
        _logger.LogCall(this);

        _ = expense.WhenNotNull();

        var hasEnded = expense.EndDate.HasValue && expense.EndDate.Value <= asOfDate;

        return !expense.ExcludeFromCalcs && !hasEnded;
    }

    public Task MarkDirtyForAccountAsync(AccountEntity account, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        _ = account.WhenNotNull();

        return MarkDirtyForAccountsAsync([account], cancellationToken);
    }

    public Task MarkDirtyForExpensesAsync(IReadOnlyCollection<ExpenseEntity> expenses, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        _ = expenses.WhenNotNull();

        var expenseAccounts = expenses
            .Select(expense => expense.Account)
            .DistinctBy(account => account.Id)
            .ToArray();

        return MarkDirtyForAccountsAsync(expenseAccounts, cancellationToken);
    }

    private async Task MarkDirtyForAccountsAsync(AccountEntity[] accounts, CancellationToken cancellationToken)
    {
        if (accounts.Length == 0)
        {
            return;
        }

        var accountIds = accounts
            .Select(account => account.Id)
            .ToArray();

        var existingAccountAccruals = await _accountAccrualRepository.AccountAccruals
            .Where(item => accountIds.Contains(item.AccountId))
            .ToDictionaryAsync(item => item.AccountId, cancellationToken)
            .ConfigureAwait(false);

        foreach (var account in accounts)
        {
            if (!existingAccountAccruals.TryGetValue(account.Id, out var accountAccrual))
            {
                accountAccrual = new AccountAccrualEntity
                {
                    AccountId = account.Id,

                    // Intentionally assigning only AccountId (FK) and not assigning the Account navigation.
                    //
                    // Why this is required:
                    // - This concern can be called in flows where the DbContext is already tracking
                    //   account/expense graphs loaded by other repositories in the same request.
                    // - If we assign Account = account here, EF Core may walk that navigation graph during Add(),
                    //   attempt to attach related entities, and collide with instances already tracked in the
                    //   current context.
                    // - Those collisions can surface as identity tracking conflicts (same key, different instance),
                    //   which are runtime correctness issues, not just test-only artifacts.
                    //
                    // Why FK-only is the correct pattern here:
                    // - Persisting this row only requires AccountId; the Account navigation is not needed for this
                    //   write path.
                    // - FK-only insertion keeps the operation narrowly scoped to AccountAccrual and avoids
                    //   accidental graph-attach side effects.
                    // - It also preserves the intended transaction boundary where the caller coordinates SaveAsync.

                    AccruedIsDirty = true
                };

                _accountAccrualRepository.Add(accountAccrual);

                continue;
            }

            if (!accountAccrual.AccruedIsDirty)
            {
                accountAccrual.AccruedIsDirty = true;

                _accountAccrualRepository.Update(accountAccrual);
            }
        }
    }
}
