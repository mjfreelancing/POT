using AllOverIt.Assertion;
using AllOverIt.Pagination;
using Pot.Data.Extensions;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Repositories.Expenses;

namespace Pot.Data.UnitOfWork;

internal sealed class PotUnitOfWork : UnitOfWork<PotDbContext>, IPotUnitOfWork
{
    private readonly IQueryPaginatorFactory _queryPaginatorFactory;
    private readonly Lazy<IAccountRepository> _accountRepository;
    private readonly Lazy<IExpenseRepository> _expenseRepository;

    public IAccountRepository AccountRepository => _accountRepository.Value;
    public IExpenseRepository ExpenseRepository => _expenseRepository.Value;

    public PotUnitOfWork(PotDbContext dbContext, IQueryPaginatorFactory queryPaginatorFactory)
        : base(dbContext)
    {
        _queryPaginatorFactory = queryPaginatorFactory.WhenNotNull();

        _accountRepository = new Lazy<IAccountRepository>(() => new AccountRepository(DbContext));
        _expenseRepository = new Lazy<IExpenseRepository>(() => new ExpenseRepository(DbContext, _queryPaginatorFactory));
    }

    public IDisposable WithTracking() => DbContext.WithAutoTracking();
}
