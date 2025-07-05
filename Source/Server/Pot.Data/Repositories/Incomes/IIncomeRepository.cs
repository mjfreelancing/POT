using AllOverIt.Pagination;
using Pot.Data.Entities;
using Pot.Shared;

namespace Pot.Data.Repositories.Incomes;

public interface IIncomeRepository : IGenericRepository<PotDbContext, IncomeEntity>
{
    Task<List<IncomeEntity>> GetAllIncomesAsync(CancellationToken cancellationToken);
    Task<PageResult<IncomeEntity>> GetAllIncomesPagedAsync(Paging paging, CancellationToken cancellationToken);
    Task<List<IncomeEntity>> GetIncomesAsync(Guid[] rowIds, CancellationToken cancellationToken);
    Task<IncomeEntity?> GetIncomeOrDefaultAsync(Guid incomeId, CancellationToken cancellationToken);
}
