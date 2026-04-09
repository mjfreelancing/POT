using AllOverIt.Assertion;
using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;
using Pot.Data.Extensions;
using Pot.Data.Specifications;

namespace Pot.Data.Repositories.Expenses;

internal sealed class ExpenseRepository : PersistableRepository, IPersistableExpenseRepository
{
    public IQueryable<ExpenseEntity> Expenses => _dbContext.Expenses;

    public ExpenseRepository(PotDbContext dbContext)
        : base(dbContext)
    {
        _ = dbContext.WhenNotNull();
    }

    public Task<List<ExpenseEntity>> GetAllExpensesAsync(CancellationToken cancellationToken)
    {
        return Expenses
            .Include(expense => expense.Account)
            .ToListAsync(cancellationToken);
    }

    public Task<ExpenseEntity?> GetExpenseOrDefaultAsync(Guid rowId, CancellationToken cancellationToken)
    {
        return Expenses
            .Include(expense => expense.Account)
            .SingleOrDefaultAsync(rowId, cancellationToken);
    }

    public Task<List<ExpenseEntity>> GetExpensesAsync(Guid[] rowIds, CancellationToken cancellationToken)
    {
        return Expenses
            .Include(expense => expense.Account)
            .Where(expense => rowIds.Contains(expense.RowId))
            .ToListAsync(cancellationToken);
    }

    public Task<List<ExpenseEntity>> GetExpensesForAccountAsync(Guid accountRowId, CancellationToken cancellationToken)
    {
        // Don't exclude expenses that are marked as ExcludeFromCalcs as they may still be relevant to the caller,
        // such as updating accruals when toggling the flag.
        return Expenses
            .Include(expense => expense.Account)
            .Where(expense => expense.Account.RowId == accountRowId)
            .ToListAsync(cancellationToken);
    }

    public Task<Guid[]> GetRequiredRenewalsAsync(Guid[] accountRowIds, DateOnly asOfDate, CancellationToken cancellationToken)
    {
        var isInAccountSet = ExpenseSpecifications.IsInAccountSet(accountRowIds).Expression;
        var requiresRenewal = ExpenseSpecifications.RequiresRenewal(asOfDate).Expression;

        return Expenses
            .Where(isInAccountSet)
            .Where(requiresRenewal)
            .Select(expense => expense.RowId)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Guid[]> GetRequiredAccountAccrualsAsync(Guid[] accountRowIds, DateOnly asOfDate, CancellationToken cancellationToken)
    {
        var isInAccountSet = ExpenseSpecifications.IsInAccountSet(accountRowIds).Expression;
        var requiresAccrualUpdate = ExpenseSpecifications.RequiresAccrualUpdate(asOfDate).Expression;

        // Not excluding expenses marked as ExcludeFromCalcs as they may still require an accrual update,
        // such as when toggling the flag.
        return Expenses
            .Where(isInAccountSet)
            .Where(requiresAccrualUpdate)
            .Select(expense => expense.Account.RowId)
            .Distinct()
            .ToArrayAsync(cancellationToken);
    }
}
