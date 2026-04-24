using AllOverIt.Assertion;
using Microsoft.EntityFrameworkCore;
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

    public Task<Guid[]> GetRequiredAccountAccrualsAsync(Guid[] accountRowIds, DateOnly asOfDate, CancellationToken cancellationToken)
    {
        return AccountAccruals
            .Where(item => accountRowIds.Contains(item.Account.RowId))
            .Where(item => item.AccruedIsDirty || item.LastAccruedDate == null || item.LastAccruedDate < asOfDate)
            .Select(item => item.Account.RowId)
            .ToArrayAsync(cancellationToken);
    }
}
