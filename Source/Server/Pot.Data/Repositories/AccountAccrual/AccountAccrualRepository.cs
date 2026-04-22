using AllOverIt.Assertion;
using Microsoft.Extensions.Logging;
using Pot.Data.Entities;

namespace Pot.Data.Repositories.AccountAccrual;

internal sealed class AccountAccrualRepository : PersistableRepository, IPersistableAccountAccrualRepository
{
    public IQueryable<AccountAccrualEntity> AccountAccruals => _dbContext.AccountAccruals;

    public ILogger<AccountAccrualRepository> Logger { get; }

    public AccountAccrualRepository(PotDbContext dbContext, ILogger<AccountAccrualRepository> logger)
        : base(dbContext)
    {
        Logger = logger.WhenNotNull();
    }
}
