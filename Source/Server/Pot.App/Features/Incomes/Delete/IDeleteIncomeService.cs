using AllOverIt.Patterns.Result;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Incomes.Delete;

public interface IDeleteIncomeService : IPotScopedDependency
{
    Task<EnrichedResult<bool>> DeleteIncomeAsync(Guid incomeId, CancellationToken cancellationToken);
}
