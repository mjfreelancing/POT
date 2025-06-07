using AllOverIt.Assertion;
using AllOverIt.EntityFrameworkCore.Pagination.Extensions;
using AllOverIt.Pagination;
using AllOverIt.Pagination.Extensions;
using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;
using Pot.Data.Extensions;
using Pot.Shared;

namespace Pot.Data.Repositories.Incomes;

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

        // The OrderBy needs Description + Id to ensure pagination works correctly
        return _queryPaginatorFactory
            .CreatePaginator(incomeQuery, paginatorConfig)
            .ColumnAscending(entity => entity.NextDue, entity => entity.Description, entity => entity.Id)
            .GetPageResultsAsync(paging.Continuation, cancellationToken);
    }

    public Task<IncomeEntity?> GetIncomeOrDefaultAsync(Guid id, CancellationToken cancellationToken)
    {
        return AsQueryable()
            .Include(income => income.Account)
            .SingleOrDefaultAsync(id, cancellationToken);
    }
}
