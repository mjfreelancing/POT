using Pot.Data.Entities;

namespace Pot.Data.Repositories.Projections;

public interface IProjectionsRepository : IRepositoryBase
{
    // Excluded incomes and expenses are not included in the returned accounts.
    Task<List<AccountEntity>> GetAllAccountsWithCandidateIncomesAndExpensesAsync(CancellationToken cancellationToken);
}
