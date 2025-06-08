using AllOverIt.Patterns.Result;
using Pot.App.Concerns.DependencyInjection;

namespace Pot.App.Features.Incomes.Delete;

public interface IDeleteIncomeService : IPotScopedDependency
{
    Task<EnrichedResult<bool>> DeleteIncomeAsync(Guid incomeId, CancellationToken cancellationToken);
}
