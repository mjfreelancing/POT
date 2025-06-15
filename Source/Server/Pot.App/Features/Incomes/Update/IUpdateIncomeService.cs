using AllOverIt.Patterns.Result;
using Pot.App.Features.Incomes.Update.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Incomes.Update;

public interface IUpdateIncomeService : IPotScopedDependency
{
    Task<EnrichedResult<Output>> UpdateIncomeAsync(Input input, CancellationToken cancellationToken);
}
