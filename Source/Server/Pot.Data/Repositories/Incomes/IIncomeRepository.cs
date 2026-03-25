using Pot.Data.Entities;

namespace Pot.Data.Repositories.Incomes;

public interface IIncomeRepository : IRepositoryBase
{
    IQueryable<IncomeEntity> Incomes { get; }

    Task<List<IncomeEntity>> GetAllIncomesAsync(CancellationToken cancellationToken);
    Task<List<IncomeEntity>> GetIncomesAsync(Guid[] rowIds, CancellationToken cancellationToken);
    Task<IncomeEntity?> GetIncomeOrDefaultAsync(Guid incomeId, CancellationToken cancellationToken);
    Task<Guid[]> GetRequiredRenewalsAsync(Guid[] accountRowIds, DateOnly asOfDate, CancellationToken cancellationToken);
}
