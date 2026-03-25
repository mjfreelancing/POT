using AllOverIt.Pagination;
using Pot.Data.Entities;
using Pot.Shared;

namespace Pot.Data.Repositories.Expenses;

public interface IExpenseRepository : IRepositoryBase
{
    IQueryable<ExpenseEntity> Expenses { get; }

    Task<List<ExpenseEntity>> GetAllExpensesAsync(CancellationToken cancellationToken);
    Task<PageResult<ExpenseEntity>> GetAllExpensesPagedAsync(Paging paging, CancellationToken cancellationToken);
    Task<ExpenseEntity?> GetExpenseOrDefaultAsync(Guid rowId, CancellationToken cancellationToken);
    Task<List<ExpenseEntity>> GetExpensesAsync(Guid[] rowIds, CancellationToken cancellationToken);
    Task<List<ExpenseEntity>> GetExpensesForAccountAsync(Guid accountRowId, CancellationToken cancellationToken);
    Task<Guid[]> GetRequiredRenewalsAsync(Guid[] accountRowIds, DateOnly asOfDate, CancellationToken cancellationToken);
    Task<Guid[]> GetRequiredAccountAccrualsAsync(Guid[] accountRowIds, DateOnly asOfDate, CancellationToken cancellationToken);
}
