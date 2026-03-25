using AllOverIt.Assertion;
using AllOverIt.EntityFrameworkCore.Pagination.Extensions;
using AllOverIt.Pagination;
using AllOverIt.Pagination.Extensions;
using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;
using Pot.Data.Extensions;
using Pot.Data.Specifications;
using Pot.Shared;
using Pot.Shared.Enumerations;

namespace Pot.Data.Repositories.Expenses;

internal sealed class ExpenseRepository : PersistableRepository, IPersistableExpenseRepository
{
    private readonly IQueryPaginatorFactory _queryPaginatorFactory;

    public IQueryable<ExpenseEntity> Expenses => _dbContext.Expenses;

    public ExpenseRepository(PotDbContext dbContext, IQueryPaginatorFactory queryPaginatorFactory)
        : base(dbContext)
    {
        _queryPaginatorFactory = queryPaginatorFactory.WhenNotNull();
    }

    public Task<List<ExpenseEntity>> GetAllExpensesAsync(CancellationToken cancellationToken)
    {
        return Expenses
            .Include(expense => expense.Account)
            .ToListAsync(cancellationToken);
    }

    public Task<PageResult<ExpenseEntity>> GetAllExpensesPagedAsync(Paging paging, CancellationToken cancellationToken)
    {
        var incomeQuery = Expenses.Include(expense => expense.Account);

        var paginatorConfig = new QueryPaginatorConfiguration
        {
            PageSize = paging.Limit,
            PaginationDirection = PaginationDirection.Forward,
            UseParameterizedQueries = true,
            ContinuationTokenOptions =
            {
                IncludeHash = true,
                UseCompression = false
            }
        };

        // The OrderBy needs Description + Id to ensure pagination works correctly
        return _queryPaginatorFactory
            .CreatePaginator(incomeQuery, paginatorConfig)
            .ColumnAscending(entity => entity.NextDue, entity => entity.Description, entity => entity.Id)
            .GetPageResultsAsync(paging.Continuation, cancellationToken);
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
        return Expenses
            .Where(expense => accountRowIds.Contains(expense.Account.RowId))
            .Where(expense =>
                !expense.ExcludeFromCalcs &&
                expense.Frequency != Frequency.OneTime &&
                expense.NextDue <= asOfDate &&
                (!expense.EndDate.HasValue || expense.EndDate > asOfDate))
            .Select(expense => expense.RowId)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Guid[]> GetRequiredAccountAccrualsAsync(Guid[] accountRowIds, DateOnly asOfDate, CancellationToken cancellationToken)
    {
        var requiresAccrualUpdate = ExpenseSpecifications.RequiresAccrualUpdate(asOfDate).Expression;

        // Not excluding expenses marked as ExcludeFromCalcs as they may still require an accrual update,
        // such as when toggling the flag.
        return Expenses
            .Where(requiresAccrualUpdate)
            .Where(expense => accountRowIds.Contains(expense.Account.RowId))
            .Select(expense => expense.Account.RowId)
            .Distinct()
            .ToArrayAsync(cancellationToken);
    }
}
