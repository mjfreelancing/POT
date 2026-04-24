using AllOverIt.Assertion;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pot.Data.Entities;
using Pot.Data.Specifications;

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
        var isInAccountSet = AccountAccrualSpecifications.IsInAccountSet(accountRowIds).Expression;
        var requiresAccrualUpdate = AccountAccrualSpecifications.RequiresAccrualUpdate(asOfDate).Expression;

        return AccountAccruals
            .Where(isInAccountSet)
            .Where(requiresAccrualUpdate)
            .Select(item => item.Account.RowId)
            .ToArrayAsync(cancellationToken);
    }
}
