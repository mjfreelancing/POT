using Pot.Data.Repositories.Accounts;

namespace Pot.Data.UnitOfWork;

public interface IPotUnitOfWork : IUnitOfWork
{
    IAccountRepository AccountRepository { get; }
}
