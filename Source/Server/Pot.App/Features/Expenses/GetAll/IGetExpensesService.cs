using AllOverIt.Pagination;
using Pot.App.Features.Expenses.GetAll.Models;
using Pot.Shared;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Expenses.GetAll;

public interface IGetExpensesService : IPotScopedDependency
{
    Task<List<Output>> GetAllExpensesAsync(CancellationToken cancellationToken);
    Task<PageResult<Output>> GetAllExpensesAsync(Paging paging, CancellationToken cancellationToken);
}
