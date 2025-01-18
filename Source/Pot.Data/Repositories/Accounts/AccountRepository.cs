using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;

namespace Pot.Data.Repositories.Accounts;

internal sealed class AccountRepository : RepositoryBase<AccountEntity>, IAccountRepository
{
    public AccountRepository(IDbContextFactory<PotDbContext> dbContextFactory)
        : base(dbContextFactory)
    {
    }
}

