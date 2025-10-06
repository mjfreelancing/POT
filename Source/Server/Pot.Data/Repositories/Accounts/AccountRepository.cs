using AllOverIt.Assertion;
using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;
using Pot.Data.Extensions;
using Pot.Data.Repositories.Accounts.Dtos;
using Pot.Data.Specifications;

namespace Pot.Data.Repositories.Accounts;

internal sealed class AccountRepository : GenericRepository<PotDbContext, AccountEntity>, IPersistableAccountRepository
{
    private readonly ICurrentUserDataContext _currentUserDataContext;

    public AccountRepository(PotDbContext dbContext, ICurrentUserDataContext currentUserDataContext)
        : base(dbContext)
    {
        _currentUserDataContext = currentUserDataContext.WhenNotNull();
    }

    public Task<bool> AccountExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        return Current.AnyAsync(id, cancellationToken);
        // Same as:
        // return AnyAsync(EntitySpecifications.IsSameId<AccountEntity>(id).Expression, cancellationToken);
    }

    public Task<bool> HasExpensesAsync(Guid id, CancellationToken cancellationToken)
    {
        return AnyAsync(account => account.RowId == id && account.Expenses.Any(), cancellationToken);
    }

    public Task<bool> HasIncomesAsync(Guid id, CancellationToken cancellationToken)
    {
        return AnyAsync(account => account.RowId == id && account.Incomes.Any(), cancellationToken);
    }

    public Task<AccountEntity> GetAccountAsync(Guid id, CancellationToken cancellationToken)
    {
        // Same as:
        // return SingleAsync(EntitySpecifications.IsSameId<AccountEntity>(id).Expression, cancellationToken);
        return Current.SingleAsync(id, cancellationToken);
    }

    public Task<AccountEntity?> GetAccountOrDefaultAsync(Guid id, CancellationToken cancellationToken)
    {
        // Same as:
        // return AsQueryable().SingleOrDefaultAsync(id, cancellationToken);
        return SingleOrDefaultAsync(EntitySpecifications.IsSameId<AccountEntity>(id).Expression, cancellationToken);
    }

    public async Task<GetAccountDto?> GetAccountWithLinkedCountsAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _currentUserDataContext.GetUserAsync().ConfigureAwait(false);

        return await Current
            .Where(AccountSpecifications.IsSameAccount(user.Site.RowId, id).Expression)
            .Select(item => new GetAccountDto
            {
                Account = item,
                LinkedIncomes = item.Incomes.Count,
                LinkedExpenses = item.Expenses.Count
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<GetAccountDto[]> GetAllAccountsWithLinkedCountsAsync(CancellationToken cancellationToken)
    {
        return Current
            .Select(item => new GetAccountDto
            {
                Account = item,
                LinkedIncomes = item.Incomes.Count,
                LinkedExpenses = item.Expenses.Count
            })
            .ToArrayAsync(cancellationToken);
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
