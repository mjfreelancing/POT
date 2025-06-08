using AllOverIt.Patterns.Result;
using Pot.App.Concerns.DependencyInjection;
using Pot.App.Features.Incomes.Update.Models;

namespace Pot.App.Features.Incomes.Update;

public interface IUpdateIncomeService : IPotScopedDependency
{
    Task<EnrichedResult<Output>> UpdateIncomeAsync(Input input, CancellationToken cancellationToken);
}
