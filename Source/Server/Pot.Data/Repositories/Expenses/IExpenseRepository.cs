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
    Task<List<ExpenseEntity>> GetExpensesForAccountAsync(Guid accountRowId, CancellationToken cancellationToken);
}
