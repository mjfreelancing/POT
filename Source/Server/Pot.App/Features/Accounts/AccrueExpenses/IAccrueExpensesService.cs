using AllOverIt.Patterns.Result;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accounts.AccrueExpenses;

public interface IAccrueExpensesService : IPotScopedDependency
{
    Task<EnrichedResult<bool>> AccrueAsync(Guid accountRowId, CancellationToken cancellationToken);
}
