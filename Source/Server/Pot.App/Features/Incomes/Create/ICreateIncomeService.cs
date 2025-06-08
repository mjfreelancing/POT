using AllOverIt.Patterns.Result;
using Pot.App.Concerns.DependencyInjection;
using Pot.App.Features.Incomes.Create.Models;

namespace Pot.App.Features.Incomes.Create;

public interface ICreateIncomeService : IPotScopedDependency
{
    Task<EnrichedResult<Output>> CreateIncomeAsync(Input input, CancellationToken cancellationToken);
}
