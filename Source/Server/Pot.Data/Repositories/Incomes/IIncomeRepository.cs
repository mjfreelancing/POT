using AllOverIt.Pagination;
using Pot.Data.Entities;
using Pot.Shared;

namespace Pot.Data.Repositories.Incomes;

public interface IIncomeRepository : IGenericRepository<PotDbContext, IncomeEntity>
{
    Task<PageResult<IncomeEntity>> GetAllIncomesAsync(Paging paging, CancellationToken cancellationToken);
    Task<IncomeEntity?> GetIncomeOrDefaultAsync(Guid incomeId, CancellationToken cancellationToken);
}
