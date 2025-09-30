using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;
using Pot.Data.Extensions;

namespace Pot.Data.Repositories.Incomes;

internal sealed class IncomeRepository : GenericRepository<PotDbContext, IncomeEntity>, IPersistableIncomeRepository
{
    public IncomeRepository(PotDbContext dbContext)
        : base(dbContext)
    {
    }

    public Task<List<IncomeEntity>> GetAllIncomesAsync(CancellationToken cancellationToken)
    {
        return Current
            .Include(income => income.Account)
            .ToListAsync(cancellationToken);
    }

    public Task<List<IncomeEntity>> GetIncomesAsync(Guid[] rowIds, CancellationToken cancellationToken)
    {
        return Current
            .Include(income => income.Account)
            .Where(income => rowIds.Contains(income.RowId))
            .ToListAsync(cancellationToken);
    }

    public Task<IncomeEntity?> GetIncomeOrDefaultAsync(Guid incomeId, CancellationToken cancellationToken)
    {
        return Current
            .Include(income => income.Account)
            .SingleOrDefaultAsync(incomeId, cancellationToken);
    }

    public Task<Guid[]> GetRequiredRenewalsAsync(Guid[] accountRowIds, DateOnly beforeDate, CancellationToken cancellationToken)
    {
        return Current
            .Where(income => accountRowIds.Contains(income.Account.RowId) && income.NextDue < beforeDate)
            .Select(income => income.RowId)
            .ToArrayAsync(cancellationToken);
    }
}
