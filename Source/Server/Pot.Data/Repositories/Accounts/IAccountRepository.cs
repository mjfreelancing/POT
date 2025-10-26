using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts.Dtos;

namespace Pot.Data.Repositories.Accounts;

public interface IAccountRepository : IRepositoryBase
{
    IQueryable<AccountEntity> Accounts { get; }

    Task<bool> AccountExistsAsync(Guid rowId, CancellationToken cancellationToken);
    Task<bool> HasExpensesAsync(Guid rowId, CancellationToken cancellationToken);
    Task<bool> HasIncomesAsync(Guid rowId, CancellationToken cancellationToken);
    Task<AccountEntity> GetAccountAsync(Guid rowId, CancellationToken cancellationToken);
    Task<AccountEntity?> GetAccountOrDefaultAsync(Guid rowId, CancellationToken cancellationToken);
    Task<GetAccountDto?> GetAccountWithLinkedCountsOrDefaultAsync(Guid rowId, CancellationToken cancellationToken);
    Task<GetAccountDto[]> GetAllAccountsWithLinkedCountsAsync(CancellationToken cancellationToken);
    Task<bool> AccountExistsAsync(string bsb, string number, CancellationToken cancellationToken);
    Task<AccountEntity?> GetAccountOrDefaultAsync(string bsb, string number, CancellationToken cancellationToken);
}
