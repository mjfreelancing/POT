using AllOverIt.Assertion;
using AllOverIt.EntityFrameworkCore.Pagination.Extensions;
using AllOverIt.Pagination;
using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;
using Pot.Data.Models;

namespace Pot.Data.Repositories.Incomes;

public interface IIncomeRepository : IGenericRepository<PotDbContext, IncomeEntity>
{
    Task<PageResult<IncomeEntity>> GetAllIncomesAsync(Paging paging, CancellationToken cancellationToken);
}

internal sealed class IncomeRepository : GenericRepository<PotDbContext, IncomeEntity>, IPersistableIncomeRepository
{
    private readonly IQueryPaginatorFactory _queryPaginatorFactory;

    public IncomeRepository(PotDbContext dbContext, IQueryPaginatorFactory queryPaginatorFactory)
        : base(dbContext)
    {
        _queryPaginatorFactory = queryPaginatorFactory.WhenNotNull();
    }

    public Task<PageResult<IncomeEntity>> GetAllIncomesAsync(Paging paging, CancellationToken cancellationToken)
    {
        var incomeQuery = AsQueryable().Include(income => income.Account);

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
            .CreatePaginator(incomeQuery, paginatorConfig)
            .ColumnAscending(entity => entity.NextDue)
            .GetPageResultsAsync(paging.Continuation, cancellationToken);
    }

}
