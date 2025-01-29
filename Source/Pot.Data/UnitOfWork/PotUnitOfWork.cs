using Pot.Data.Extensions;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Repositories.Expenses;

namespace Pot.Data.UnitOfWork;

internal sealed class PotUnitOfWork : UnitOfWork<PotDbContext>, IPotUnitOfWork
{
    private Lazy<IAccountRepository> _accountRepository;
    private Lazy<IExpenseRepository> _expenseRepository;

    public IAccountRepository AccountRepository => _accountRepository.Value;
    public IExpenseRepository ExpenseRepository => _expenseRepository.Value;

    public PotUnitOfWork(PotDbContext dbContext)
        : base(dbContext)
    {
        _accountRepository = new Lazy<IAccountRepository>(() => new AccountRepository(DbContext));
        _expenseRepository = new Lazy<IExpenseRepository>(() => new ExpenseRepository(DbContext));
    }

    public IDisposable WithTracking() => DbContext.WithAutoTracking();
}
