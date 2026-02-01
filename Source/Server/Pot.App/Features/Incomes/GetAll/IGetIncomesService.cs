using Pot.App.Features.Incomes.GetAll.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Incomes.GetAll;

public interface IGetIncomesService : IPotScopedDependency
{
    Task<List<Output>> GetAllIncomesAsync(CancellationToken cancellationToken);
}
