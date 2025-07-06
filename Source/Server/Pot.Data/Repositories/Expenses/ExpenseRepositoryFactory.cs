namespace Pot.Data.Repositories.Expenses;

//internal sealed class ExpenseRepositoryFactory : IExpenseRepositoryFactory
//{
//    private readonly IDbContextFactory<PotDbContext> _dbContextFactory;
//    private readonly IQueryPaginatorFactory _queryPaginatorFactory;

//    public ExpenseRepositoryFactory(IDbContextFactory<PotDbContext> dbContextFactory, IQueryPaginatorFactory queryPaginatorFactory)
//    {
//        _dbContextFactory = dbContextFactory.WhenNotNull();
//        _queryPaginatorFactory = queryPaginatorFactory.WhenNotNull();
//    }

//    public IExpenseRepository CreateExpenseRepository()
//    {
//        return new ExpenseRepository(_dbContextFactory.CreateDbContext(), _queryPaginatorFactory);
//    }
//}
