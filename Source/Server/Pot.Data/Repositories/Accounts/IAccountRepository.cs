using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts.Dtos;

namespace Pot.Data.Repositories.Accounts;

public interface IAccountRepository : IGenericRepository<PotDbContext, AccountEntity>
{
    Task<bool> AccountExistsAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> HasExpensesAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> HasIncomesAsync(Guid id, CancellationToken cancellationToken);
    Task<AccountEntity> GetAccountAsync(Guid id, CancellationToken cancellationToken);
    Task<AccountEntity?> GetAccountOrDefaultAsync(Guid id, CancellationToken cancellationToken);
    Task<GetAccountDto?> GetAccountWithLinkedCountsAsync(Guid id, CancellationToken cancellationToken);
    Task<GetAccountDto[]> GetAllAccountsWithLinkedCountsAsync(CancellationToken cancellationToken);
    Task<bool> AccountExistsAsync(string bsb, string number, CancellationToken cancellationToken);
    Task<AccountEntity?> GetAccountOrDefaultAsync(string bsb, string number, CancellationToken cancellationToken);
    Task<List<AccountEntity>> GetAccountsWithIncomesAsync(Guid[] rowIds, CancellationToken cancellationToken);
    Task<List<AccountEntity>> GetAccountsWithExpensesAsync(Guid[] rowIds, CancellationToken cancellationToken);
    Task<List<AccountEntity>> GetAllAccountsWithIncomesAndExpensesAsync(CancellationToken cancellationToken);
}
