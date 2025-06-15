//using AllOverIt.Assertion;
//using AllOverIt.Pagination;
//using Microsoft.EntityFrameworkCore;

//namespace Pot.Data.UnitOfWork;

//internal sealed class PotUnitOfWorkFactory : IPotUnitOfWorkFactory
//{
//    private readonly IDbContextFactory<PotDbContext> _dbContextFactory;
//    private readonly IQueryPaginatorFactory _queryPaginatorFactory;

//    public PotUnitOfWorkFactory(IDbContextFactory<PotDbContext> dbContextFactory, IQueryPaginatorFactory queryPaginatorFactory)
//    {
//        _dbContextFactory = dbContextFactory.WhenNotNull();
//        _queryPaginatorFactory = queryPaginatorFactory.WhenNotNull();
//    }

//    public IPotUnitOfWork Create()
//    {
//        var dbContext = _dbContextFactory.CreateDbContext();

//        return new PotUnitOfWork(dbContext, _queryPaginatorFactory);
//    }
//}