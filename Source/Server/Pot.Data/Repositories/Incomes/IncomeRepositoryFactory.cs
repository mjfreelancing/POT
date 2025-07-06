namespace Pot.Data.Repositories.Incomes;

//internal sealed class IncomeRepositoryFactory : IIncomeRepositoryFactory
//{
//    private readonly IDbContextFactory<PotDbContext> _dbContextFactory;
//    private readonly IQueryPaginatorFactory _queryPaginatorFactory;

//    public IncomeRepositoryFactory(IDbContextFactory<PotDbContext> dbContextFactory, IQueryPaginatorFactory queryPaginatorFactory)
//    {
//        _dbContextFactory = dbContextFactory.WhenNotNull();
//        _queryPaginatorFactory = queryPaginatorFactory.WhenNotNull();
//    }

//    public IIncomeRepository CreateIncomeRepository()
//    {
//        return new IncomeRepository(_dbContextFactory.CreateDbContext(), _queryPaginatorFactory);
//    }
//}
