using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;

namespace Pot.Data.Repositories.Projections;

internal sealed class ProjectionsRepository : RepositoryBase, IProjectionsRepository
{
    public ProjectionsRepository(PotDbContext dbContext)
        : base(dbContext)
    {
    }

    public Task<List<AccountEntity>> GetAllAccountsWithCandidateIncomesAndExpensesAsync(CancellationToken cancellationToken)
    {
        return Set<AccountEntity>()
            .Include(account => account.Incomes.Where(income => !income.ExcludeFromCalcs))
            .Include(account => account.Expenses.Where(expense => !expense.ExcludeFromCalcs))
            .ToListAsync(cancellationToken);
    }
}
