namespace Pot.Data.UnitOfWork;

//internal sealed class PotUnitOfWorkFactory : IPotUnitOfWorkFactory
//{
//    private readonly IDbContextFactory<PotDbContext> _dbContextFactory;

//    public PotUnitOfWorkFactory(IDbContextFactory<PotDbContext> dbContextFactory)
//    {
//        _dbContextFactory = dbContextFactory.WhenNotNull();
//    }

//    public IPotUnitOfWork Create()
//    {
//        var dbContext = _dbContextFactory.CreateDbContext();

//        return new PotUnitOfWork(dbContext);
//    }
//}