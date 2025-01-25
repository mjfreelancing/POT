using Pot.Data.Entities;

namespace Pot.Data.Repositories.Accounts;

public interface IAccountRepository : IGenericRepository<PotDbContext, AccountEntity>
{
    Task<bool> AccountExistsAsync(Guid id, CancellationToken cancellationToken);
    Task<AccountEntity> GetAccountAsync(Guid id, CancellationToken cancellationToken);
    Task<AccountEntity?> GetAccountOrDefaultAsync(Guid id, CancellationToken cancellationToken);
    Task<AccountEntity?> GetAccountOrDefaultAsync(string bsb, string number, CancellationToken cancellationToken);
    Task<bool> AccountExistsAsync(string bsb, string number, CancellationToken cancellationToken);
}
