using Pot.Data.Entities;

namespace Pot.Data.Repositories.AccountAccrual;

internal sealed class AccountAccrualRepository : PersistableRepository, IPersistableAccountAccrualRepository
{
    public IQueryable<AccountAccrualEntity> AccountAccruals => _dbContext.AccountAccruals;

    public AccountAccrualRepository(PotDbContext dbContext)
        : base(dbContext)
    {
    }
}
