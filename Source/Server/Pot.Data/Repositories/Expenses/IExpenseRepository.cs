using AllOverIt.Pagination;
using Pot.Data.Entities;
using Pot.Shared;

namespace Pot.Data.Repositories.Expenses;

public interface IExpenseRepository : IGenericRepository<PotDbContext, ExpenseEntity>
{
    Task<List<ExpenseEntity>> GetAllExpensesAsync(CancellationToken cancellationToken);
    Task<PageResult<ExpenseEntity>> GetAllExpensesPagedAsync(Paging paging, CancellationToken cancellationToken);
    Task<ExpenseEntity?> GetExpenseOrDefaultAsync(Guid rowId, CancellationToken cancellationToken);
    Task<List<ExpenseEntity>> GetExpensesAsync(Guid[] rowIds, CancellationToken cancellationToken);
    Task<List<ExpenseEntity>> GetRenewableExpensesForAccountAsync(Guid accountRowId, CancellationToken cancellationToken);
    Task<Guid[]> GetRequiredRenewalsAsync(Guid[] accountRowId, DateOnly beforeDate, CancellationToken cancellationToken);
    Task<Guid[]> GetRequiredAccountAccrualsAsync(Guid[] accountRowId, DateOnly beforeDate, CancellationToken cancellationToken);
}
