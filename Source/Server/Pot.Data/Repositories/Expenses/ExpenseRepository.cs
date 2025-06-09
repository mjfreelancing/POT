using AllOverIt.Assertion;
using AllOverIt.EntityFrameworkCore.Pagination.Extensions;
using AllOverIt.Pagination;
using AllOverIt.Pagination.Extensions;
using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;
using Pot.Data.Extensions;
using Pot.Shared;

namespace Pot.Data.Repositories.Expenses;

internal sealed class ExpenseRepository : GenericRepository<PotDbContext, ExpenseEntity>, IPersistableExpenseRepository
{
    private readonly IQueryPaginatorFactory _queryPaginatorFactory;

    public ExpenseRepository(PotDbContext dbContext, IQueryPaginatorFactory queryPaginatorFactory)
        : base(dbContext)
    {
        _queryPaginatorFactory = queryPaginatorFactory.WhenNotNull();
    }

    public Task<PageResult<ExpenseEntity>> GetAllExpensesAsync(Paging paging, CancellationToken cancellationToken)
    {
        var incomeQuery = AsQueryable().Include(expense => expense.Account);

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

    public Task<ExpenseEntity?> GetExpenseOrDefaultAsync(Guid expenseId, CancellationToken cancellationToken)
    {
        return AsQueryable()
            .Include(expense => expense.Account)
            .SingleOrDefaultAsync(expenseId, cancellationToken);
    }
}
