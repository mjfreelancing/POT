using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;

namespace Pot.Data.Repositories.Projections;

internal sealed class ProjectionsRepository : GenericRepository<PotDbContext, AccountEntity>, IProjectionsRepository
{
    public ProjectionsRepository(PotDbContext dbContext)
        : base(dbContext)
    {
    }

    public Task<List<AccountEntity>> GetAllAccountsWithCandidateIncomesAndExpensesAsync(CancellationToken cancellationToken)
    {
        return Current
            .Include(account => account.Incomes.Where(income => !income.ExcludeFromCalcs))
            .Include(account => account.Expenses.Where(expense => !expense.ExcludeFromCalcs))
            .ToListAsync(cancellationToken);
    }
}
