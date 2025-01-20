using Pot.Data.Entities;

namespace Pot.Data.Repositories.Accounts;

public interface IAccountRepository : IGenericRepository<PotDbContext, AccountEntity>
{
    Task<AccountEntity?> FindAccountOrDefaultAsync(string bsb, string number, CancellationToken cancellationToken);
}
