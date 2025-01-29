using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;

namespace Pot.Data.Repositories.Accounts;

internal sealed class AccountRepository : GenericRepository<PotDbContext, AccountEntity>, IAccountRepository
{
    public AccountRepository(PotDbContext dbContext)
        : base(dbContext)
    {
    }

    public Task<bool> AccountExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        return Where(entity => entity.RowId == id).AnyAsync(cancellationToken);
    }

    public Task<AccountEntity> GetAccountAsync(Guid id, CancellationToken cancellationToken)
    {
        return Where(entity => entity.RowId == id).SingleAsync(cancellationToken);
    }

    public Task<AccountEntity?> GetAccountOrDefaultAsync(Guid id, CancellationToken cancellationToken)
    {
        return Where(entity => entity.RowId == id).SingleOrDefaultAsync(cancellationToken);
    }

    public Task<AccountEntity?> GetAccountOrDefaultAsync(string bsb, string number, CancellationToken cancellationToken)
    {
        return Where(entity => entity.Bsb == bsb && entity.Number == number).SingleOrDefaultAsync(cancellationToken);
    }

    public Task<bool> AccountExistsAsync(string bsb, string number, CancellationToken cancellationToken)
    {
        return Where(entity => entity.Bsb == bsb && entity.Number == number).AnyAsync(cancellationToken);
    }
}
