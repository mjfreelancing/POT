using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;
using Pot.Data.Extensions;
using Pot.Data.Repositories.Accounts.Dtos;
using Pot.Data.Specifications;

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
                LinkedExpenses = item.Expenses.Count
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
                LinkedExpenses = item.Expenses.Count
            })
            .ToArrayAsync(cancellationToken);
    }

    public Task<bool> AccountExistsAsync(string bsb, string number, CancellationToken cancellationToken)
    {
        // Account numbers are globally unique
        return Accounts
            .IgnoreQueryFilters()
            .AnyAsync(AccountSpecifications.IsSameBsbNumber(bsb, number).Expression, cancellationToken);
    }

    public Task<AccountEntity?> GetAccountOrDefaultAsync(string bsb, string number, CancellationToken cancellationToken)
    {
        return Accounts.SingleOrDefaultAsync(AccountSpecifications.IsSameBsbNumber(bsb, number).Expression, cancellationToken);
    }
}
