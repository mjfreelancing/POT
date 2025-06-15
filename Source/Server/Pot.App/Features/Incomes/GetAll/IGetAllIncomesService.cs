using AllOverIt.Pagination;
using Pot.App.Features.Incomes.GetAll.Models;
using Pot.Shared;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Incomes.GetAll;

public interface IGetAllIncomesService : IPotScopedDependency
{
    Task<PageResult<Output>> GetAllIncomesAsync(Paging paging, CancellationToken cancellationToken);
}
