using Pot.Data.Repositories.Accounts;

namespace Pot.Data.UnitOfWork;

internal sealed class PotUnitOfWork : UnitOfWork<PotDbContext>, IPotUnitOfWork
{
    private IAccountRepository? _accountRepository;

    public IAccountRepository AccountRepository
    {
        get
        {
            _accountRepository ??= new AccountRepository(DbContext);

            return _accountRepository;
        }
    }

    public PotUnitOfWork(PotDbContext dbContext)
        : base(dbContext)
    {
    }
}
