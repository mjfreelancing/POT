using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;

namespace Pot.Data.Repositories.Accounts;

internal sealed class AccountRepository : GenericRepository<PotDbContext, AccountEntity>, IAccountRepository
{
    public AccountRepository(PotDbContext dbContext)
        : base(dbContext)
    {
    }

    public Task<AccountEntity?> FindAccountOrDefaultAsync(string bsb, string number, CancellationToken cancellationToken)
    {
        return Find(entity => entity.Bsb == bsb && entity.Number == number).SingleOrDefaultAsync(cancellationToken);
    }
}
