using AllOverIt.Pagination;
using Pot.App.Concerns.DependencyInjection;
using Pot.App.Features.Expenses.GetAll.Models;
using Pot.Shared;

namespace Pot.App.Features.Expenses.GetAll;

public interface IGetAllExpensesService : IPotScopedDependency
{
    Task<PageResult<Output>> GetAllExpensesAsync(Paging paging, CancellationToken cancellationToken);
}
