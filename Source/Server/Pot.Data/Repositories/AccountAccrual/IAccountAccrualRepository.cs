using Pot.Data.Entities;

namespace Pot.Data.Repositories.AccountAccrual;

public interface IAccountAccrualRepository : IRepositoryBase
{
    IQueryable<AccountAccrualEntity> AccountAccruals { get; }

    Task<Guid[]> GetRequiredAccountAccrualsAsync(Guid[] accountRowIds, DateOnly asOfDate, CancellationToken cancellationToken);
}
