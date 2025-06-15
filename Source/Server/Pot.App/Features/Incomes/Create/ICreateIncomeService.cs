using AllOverIt.Patterns.Result;
using Pot.App.Features.Incomes.Create.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Incomes.Create;

public interface ICreateIncomeService : IPotScopedDependency
{
    Task<EnrichedResult<Output>> CreateIncomeAsync(Input input, CancellationToken cancellationToken);
}
