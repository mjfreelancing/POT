using Pot.Data.Entities;

namespace Pot.Data.Repositories.Projections;

public interface IProjectionsRepository : IGenericRepository<PotDbContext, AccountEntity>
{
    // Excluded incomes and expenses are not included in the returned accounts.
    Task<List<AccountEntity>> GetAllAccountsWithCandidateIncomesAndExpensesAsync(CancellationToken cancellationToken);
}
