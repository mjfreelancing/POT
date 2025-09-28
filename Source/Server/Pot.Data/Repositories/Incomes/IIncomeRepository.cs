using Pot.Data.Entities;

namespace Pot.Data.Repositories.Incomes;

public interface IIncomeRepository : IGenericRepository<PotDbContext, IncomeEntity>
{
    Task<List<IncomeEntity>> GetAllIncomesAsync(CancellationToken cancellationToken);
    Task<List<IncomeEntity>> GetIncomesAsync(Guid[] rowIds, CancellationToken cancellationToken);
    Task<IncomeEntity?> GetIncomeOrDefaultAsync(Guid incomeId, CancellationToken cancellationToken);
    Task<Guid[]> RenewalsRequiredAsync(Guid[] accountRowIds, DateOnly beforeDate, CancellationToken cancellationToken);
}
