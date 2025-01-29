using Pot.Data.Repositories.Accounts;
using Pot.Data.Repositories.Expenses;

namespace Pot.Data.UnitOfWork;

public interface IPotUnitOfWork : IUnitOfWork
{
    IAccountRepository AccountRepository { get; }
    IExpenseRepository ExpenseRepository { get; }

    IDisposable WithTracking();
}
