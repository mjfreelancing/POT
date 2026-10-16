using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;
using Pot.Data.Extensions;
using Pot.Data.Specifications;

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

    public Task<List<IncomeEntity>> GetIncomesAsync(IReadOnlyList<Guid> rowIds, CancellationToken cancellationToken)
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

    public async Task<IReadOnlyList<Guid>> GetRequiredRenewalsAsync(
        IReadOnlyList<Guid> accountRowIds, DateOnly asOfDate, CancellationToken cancellationToken)
    {
        var isInAccountSet = IncomeSpecifications.IsInAccountSet(accountRowIds).Expression;
        var requiresRenewal = IncomeSpecifications.RequiresRenewal(asOfDate).Expression;

        return await Incomes
            .Where(isInAccountSet)
            .Where(requiresRenewal)
            .Select(income => income.RowId)
            .ToArrayAsync(cancellationToken);
    }
}
