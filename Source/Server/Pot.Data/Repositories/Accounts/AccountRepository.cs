using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;
using Pot.Data.Extensions;
using Pot.Data.Repositories.Accounts.Dtos;
using Pot.Data.Specifications;
using Pot.Shared.Models;
using System.Linq.Expressions;

namespace Pot.Data.Repositories.Accounts;

internal sealed class AccountRepository : PersistableRepository, IPersistableAccountRepository
{
    public IQueryable<AccountEntity> Accounts => _dbContext.Accounts;

    public AccountRepository(PotDbContext dbContext)
        : base(dbContext)
    {
    }

    public Task<bool> AccountExistsAsync(Guid rowId, CancellationToken cancellationToken)
    {
        return Accounts.AnyAsync(rowId, cancellationToken);
        // Same as:
        // return AnyAsync(EntitySpecifications.IsSameId<AccountEntity>(id).Expression, cancellationToken);
    }

    public Task<bool> HasExpensesAsync(Guid rowId, CancellationToken cancellationToken)
    {
        return Accounts.AnyAsync(account => account.RowId == rowId && account.Expenses.Any(), cancellationToken);
    }

    public Task<bool> HasIncomesAsync(Guid rowId, CancellationToken cancellationToken)
    {
        return Accounts.AnyAsync(account => account.RowId == rowId && account.Incomes.Any(), cancellationToken);
    }

    public Task<AccountEntity> GetAccountAsync(Guid rowId, CancellationToken cancellationToken)
    {
        // Same as:
        // return SingleAsync(EntitySpecifications.IsSameId<AccountEntity>(id).Expression, cancellationToken);
        return Accounts.SingleAsync(rowId, cancellationToken);
    }

    public Task<AccountEntity?> GetAccountOrDefaultAsync(Guid rowId, CancellationToken cancellationToken)
    {
        // Same as:
        // return AsQueryable().SingleOrDefaultAsync(id, cancellationToken);
        return Accounts.SingleOrDefaultAsync(EntitySpecifications.IsSameId<AccountEntity>(rowId).Expression, cancellationToken);
    }

    public async Task<AccountWithLinkedCounts?> GetAccountWithLinkedCountsOrDefaultAsync(Guid rowId, CancellationToken cancellationToken)
    {
        return await Accounts
            .Where(account => account.RowId == rowId)
            .Select(item => new AccountWithLinkedCounts
            {
                Account = item,
                LinkedIncomes = item.Incomes.Count,
                LinkedExpenses = item.Expenses.Count,
                AccrualExpenses = item.Expenses
                    .AsQueryable()
                    .Where(expense => !expense.ExcludeFromCalcs)
                    .Select(AccrualExpenseProjection)
                    .ToArray()
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<AccountWithLinkedCounts[]> GetAllAccountsWithLinkedCountsAsync(CancellationToken cancellationToken)
    {
        return Accounts
            .Select(item => new AccountWithLinkedCounts
            {
                Account = item,
                LinkedIncomes = item.Incomes.Count,
                LinkedExpenses = item.Expenses.Count,
                AccrualExpenses = item.Expenses
                    .AsQueryable()
                    .Where(expense => !expense.ExcludeFromCalcs)
                    .Select(AccrualExpenseProjection)
                    .ToArray()
            })
            .ToArrayAsync(cancellationToken);
    }

    // Part of a query expression: this must stay an expression tree so EF Core can translate it into SQL, and the
    // nested collections call AsQueryable() so this overload binds. That is why it cannot be shared with the
    // in-memory projection in Pot.App. Declared once because both account queries project the same accrual facts.
    private static readonly Expression<Func<ExpenseEntity, ExpenseAccrualInput>> AccrualExpenseProjection =
        expense => new ExpenseAccrualInput
        {
            RowId = expense.RowId,
            ExcludeFromCalcs = expense.ExcludeFromCalcs,
            AccrualStart = expense.AccrualStart,
            NextDue = expense.NextDue,
            EndDate = expense.EndDate,
            AccrualPolicy = expense.AccrualPolicy,
            Frequency = expense.Frequency,
            FrequencyCount = expense.FrequencyCount,
            Amount = expense.Amount
        };
}
