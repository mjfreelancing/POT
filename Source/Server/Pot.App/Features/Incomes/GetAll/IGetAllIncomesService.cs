using AllOverIt.Pagination;
using Pot.App.Concerns.DependencyInjection;
using Pot.App.Features.Incomes.GetAll.Models;
using Pot.Shared;

namespace Pot.App.Features.Incomes.GetAll;

public interface IGetAllIncomesService : IPotScopedDependency
{
    Task<PageResult<Output>> GetAllIncomesAsync(Paging paging, CancellationToken cancellationToken);
}
