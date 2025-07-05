using AllOverIt.Patterns.Result;
using Pot.App.Features.Incomes.Renew.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Incomes.Renew;

public interface IRenewIncomesService : IPotScopedDependency
{
    Task<EnrichedResult<bool>> RenewAsync(Input input, CancellationToken cancellationToken);
}
