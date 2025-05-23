using Pot.Data.Entities;
using Pot.Data.Specifications;

namespace Pot.Data.Repositories.Accounts;

internal sealed class AccountRepository : GenericRepository<PotDbContext, AccountEntity>, IPersistableAccountRepository
{
    public AccountRepository(PotDbContext dbContext)
        : base(dbContext)
    {
    }

    public Task<bool> AccountExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        return AnyAsync(EntitySpecifications.IsSameId<AccountEntity>(id).Expression, cancellationToken);
    }

    public Task<AccountEntity> GetAccountAsync(Guid id, CancellationToken cancellationToken)
    {
        return SingleAsync(EntitySpecifications.IsSameId<AccountEntity>(id).Expression, cancellationToken);
    }

    public Task<AccountEntity?> GetAccountOrDefaultAsync(Guid id, CancellationToken cancellationToken)
    {
        return SingleOrDefaultAsync(EntitySpecifications.IsSameId<AccountEntity>(id).Expression, cancellationToken);
    }

    public Task<bool> AccountExistsAsync(string bsb, string number, CancellationToken cancellationToken)
    {
        return AnyAsync(AccountSpecifications.IsSameBsbNumber(bsb, number).Expression, cancellationToken);
    }

    public Task<AccountEntity?> GetAccountOrDefaultAsync(string bsb, string number, CancellationToken cancellationToken)
    {
        return SingleOrDefaultAsync(AccountSpecifications.IsSameBsbNumber(bsb, number).Expression, cancellationToken);
    }
}
