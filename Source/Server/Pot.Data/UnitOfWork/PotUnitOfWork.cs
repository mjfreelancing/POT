using AllOverIt.Assertion;
using Pot.Data.Extensions;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Repositories.Expenses;
using Pot.Data.Repositories.Incomes;

namespace Pot.Data.UnitOfWork;

internal sealed class PotUnitOfWork : UnitOfWork<PotDbContext>, IPotUnitOfWork
{
    private readonly Lazy<IAccountRepository> _accountRepository;
    private readonly Lazy<IExpenseRepository> _expenseRepository;
    private readonly Lazy<IIncomeRepository> _incomeRepository;

    public IAccountRepository AccountRepository => _accountRepository.Value;
    public IExpenseRepository ExpenseRepository => _expenseRepository.Value;
    public IIncomeRepository IncomeRepository => _incomeRepository.Value;

    public PotUnitOfWork(PotDbContext dbContext)
        : base(dbContext)
    {
        _ = dbContext.WhenNotNull();

        _accountRepository = new Lazy<IAccountRepository>(() => new AccountRepository(DbContext));
        _expenseRepository = new Lazy<IExpenseRepository>(() => new ExpenseRepository(DbContext));
        _incomeRepository = new Lazy<IIncomeRepository>(() => new IncomeRepository(DbContext));
    }

    public IDisposable WithTracking() => DbContext.WithAutoTracking();
}
