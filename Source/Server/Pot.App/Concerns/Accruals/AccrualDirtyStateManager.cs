using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pot.App.Concerns.Accruals.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.AccountAccrual;

namespace Pot.App.Concerns.Accruals;

internal sealed class AccrualDirtyStateManager : IAccrualDirtyStateManager
{
    private readonly IPersistableAccountAccrualRepository _accountAccrualRepository;
    private readonly ILogger<AccrualDirtyStateManager> _logger;

    public AccrualDirtyStateManager(IPersistableAccountAccrualRepository accountAccrualRepository, ILogger<AccrualDirtyStateManager> logger)
    {
        _accountAccrualRepository = accountAccrualRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public int[] GetAccountsRequiringRecalc(ExpenseAccrualState before, ExpenseAccrualState after)
    {
        _logger.LogCall(this);

        _ = before.WhenNotNull();
        _ = after.WhenNotNull();

        if (before == after)
        {
            return [];
        }

        if (before.ExcludeFromCalcs && after.ExcludeFromCalcs)
        {
            return [];
        }

        if (before.AccountId == after.AccountId)
        {
            return [after.AccountId];
        }

        return [before.AccountId, after.AccountId];
    }

    public bool IsExpenseDeletionImpactful(ExpenseEntity expense, DateOnly asOfDate)
    {
        _logger.LogCall(this);

        _ = expense.WhenNotNull();

        var hasEnded = expense.EndDate.HasValue && expense.EndDate.Value <= asOfDate;

        return !expense.ExcludeFromCalcs && !hasEnded;
    }

    public Task SetAccountsDirtyAsync(IReadOnlyCollection<int> accountIds, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        _ = accountIds.WhenNotNull();

        return SetAccountsDirtyByIdAsync(accountIds.Distinct().ToArray(), cancellationToken);
    }

    public async Task SetAccountCleanAsync(int accountId, DateOnly asOfDate, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var accountAccrual = await _accountAccrualRepository.AccountAccruals
            .SingleOrDefaultAsync(item => item.AccountId == accountId, cancellationToken)
            .ConfigureAwait(false);

        if (accountAccrual is null)
        {
            _accountAccrualRepository.Add(new AccountAccrualEntity
            {
                AccountId = accountId,
                AccruedIsDirty = false,
                LastAccruedDate = asOfDate
            });

            return;
        }

        if (accountAccrual.AccruedIsDirty || accountAccrual.LastAccruedDate != asOfDate)
        {
            accountAccrual.AccruedIsDirty = false;
            accountAccrual.LastAccruedDate = asOfDate;

            _accountAccrualRepository.Update(accountAccrual);
        }
    }

    public Task SetAccountsDirtyAsync(IReadOnlyCollection<ExpenseEntity> expenses, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        _ = expenses.WhenNotNull();

        var expenseAccounts = expenses
            .Select(expense => expense.Account)
            .DistinctBy(account => account.Id)
            .Select(account => account.Id)
            .ToArray();

        return SetAccountsDirtyByIdAsync(expenseAccounts, cancellationToken);
    }

    private async Task SetAccountsDirtyByIdAsync(int[] accountIds, CancellationToken cancellationToken)
    {
        if (accountIds.Length == 0)
        {
            return;
        }

        var existingAccountAccruals = await _accountAccrualRepository.AccountAccruals
            .Where(item => accountIds.Contains(item.AccountId))
            .ToDictionaryAsync(item => item.AccountId, cancellationToken)
            .ConfigureAwait(false);

        foreach (var accountId in accountIds)
        {
            if (!existingAccountAccruals.TryGetValue(accountId, out var accountAccrual))
            {
                accountAccrual = new AccountAccrualEntity
                {
                    AccountId = accountId,

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
