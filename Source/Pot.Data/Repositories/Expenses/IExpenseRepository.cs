using AllOverIt.Assertion;
using AllOverIt.EntityFrameworkCore.Pagination.Extensions;
using AllOverIt.Pagination;
using Pot.Data.Entities;
using Pot.Data.Models;

namespace Pot.Data.Repositories.Expenses;

public interface IExpenseRepository : IGenericRepository<PotDbContext, ExpenseEntity>
{
    //Task<bool> AccountExistsAsync(Guid id, CancellationToken cancellationToken);
    //Task<AccountEntity> GetAccountAsync(Guid id, CancellationToken cancellationToken);
    //Task<AccountEntity?> GetAccountOrDefaultAsync(Guid id, CancellationToken cancellationToken);
    //Task<AccountEntity?> GetAccountOrDefaultAsync(string bsb, string number, CancellationToken cancellationToken);
    //Task<bool> AccountExistsAsync(string bsb, string number, CancellationToken cancellationToken);


    Task<PageResult<ExpenseEntity>> GetExpensesForAccountAsync(Guid accountId, Paging paging, CancellationToken cancellationToken);
}

internal sealed class ExpenseRepository : GenericRepository<PotDbContext, ExpenseEntity>, IPersistableExpenseRepository
{
    private readonly IQueryPaginatorFactory _queryPaginatorFactory;

    public ExpenseRepository(PotDbContext dbContext, IQueryPaginatorFactory queryPaginatorFactory)
        : base(dbContext)
    {
        _queryPaginatorFactory = queryPaginatorFactory.WhenNotNull();
    }

    public Task<PageResult<ExpenseEntity>> GetExpensesForAccountAsync(Guid accountId, Paging paging, CancellationToken cancellationToken)
    {
        var expenseQuery = Where(expense => expense.Account != null && expense.Account.RowId == accountId);

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

        return _queryPaginatorFactory
            .CreatePaginator(expenseQuery, paginatorConfig)
            .ColumnAscending(entity => entity.NextDue)
            .GetPageResultsAsync(paging.Continuation, cancellationToken);
    }


    //public Task<bool> AccountExistsAsync(Guid id, CancellationToken cancellationToken)
    //{
    //    return Query().AnyAsync(entity => entity.RowId == id, cancellationToken);
    //}

    //public Task<AccountEntity> GetAccountAsync(Guid id, CancellationToken cancellationToken)
    //{
    //    return Query().SingleAsync(entity => entity.RowId == id, cancellationToken);
    //}

    //public Task<AccountEntity?> GetAccountOrDefaultAsync(Guid id, CancellationToken cancellationToken)
    //{
    //    return Query().SingleOrDefaultAsync(entity => entity.RowId == id, cancellationToken);
    //}

    //public Task<AccountEntity?> GetAccountOrDefaultAsync(string bsb, string number, CancellationToken cancellationToken)
    //{
    //    return Query().SingleOrDefaultAsync(entity => entity.Bsb == bsb && entity.Number == number, cancellationToken);
    //}

    //public Task<bool> AccountExistsAsync(string bsb, string number, CancellationToken cancellationToken)
    //{
    //    return Query().AnyAsync(entity => entity.Bsb == bsb && entity.Number == number, cancellationToken);
    //}
}
