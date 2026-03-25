using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;
using Pot.Data.Extensions;
using Pot.Shared.Enumerations;

namespace Pot.Data.Repositories.Incomes;

internal sealed class IncomeRepository : PersistableRepository, IPersistableIncomeRepository
{
    public IQueryable<IncomeEntity> Incomes => _dbContext.Incomes;

    public IncomeRepository(PotDbContext dbContext)
        : base(dbContext)
    {
    }

    public Task<List<IncomeEntity>> GetAllIncomesAsync(CancellationToken cancellationToken)
    {
        return Incomes
            .Include(income => income.Account)
            .ToListAsync(cancellationToken);
    }

    public Task<List<IncomeEntity>> GetIncomesAsync(Guid[] rowIds, CancellationToken cancellationToken)
    {
        return Incomes
            .Include(income => income.Account)
            .Where(income => rowIds.Contains(income.RowId))
            .ToListAsync(cancellationToken);
    }

    public Task<IncomeEntity?> GetIncomeOrDefaultAsync(Guid incomeId, CancellationToken cancellationToken)
    {
        return Incomes
            .Include(income => income.Account)
            .SingleOrDefaultAsync(incomeId, cancellationToken);
    }

    public Task<Guid[]> GetRequiredRenewalsAsync(Guid[] accountRowIds, DateOnly asOfDate, CancellationToken cancellationToken)
    {
        return Incomes
            .Where(income => accountRowIds.Contains(income.Account.RowId))
            .Where(income =>
                !income.ExcludeFromCalcs &&
                income.Frequency != Frequency.OneTime &&
                income.NextDue <= asOfDate &&
                (!income.EndDate.HasValue || income.EndDate > asOfDate))
            .Select(income => income.RowId)
            .ToArrayAsync(cancellationToken);
    }
}
