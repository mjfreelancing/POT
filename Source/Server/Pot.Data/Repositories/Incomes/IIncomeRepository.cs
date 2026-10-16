using Pot.Data.Entities;

namespace Pot.Data.Repositories.Incomes;

public interface IIncomeRepository : IRepositoryBase
{
    IQueryable<IncomeEntity> Incomes { get; }

    Task<List<IncomeEntity>> GetAllIncomesAsync(CancellationToken cancellationToken);
    Task<List<IncomeEntity>> GetIncomesAsync(IReadOnlyList<Guid> rowIds, CancellationToken cancellationToken);
    Task<IncomeEntity?> GetIncomeOrDefaultAsync(Guid incomeId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Guid>> GetRequiredRenewalsAsync(IReadOnlyList<Guid> accountRowIds, DateOnly asOfDate, CancellationToken cancellationToken);
}
