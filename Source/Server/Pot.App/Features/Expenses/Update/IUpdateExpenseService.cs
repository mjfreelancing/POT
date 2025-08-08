using AllOverIt.Patterns.Result;
using Pot.App.Features.Expenses.Update.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Expenses.Update;

public interface IUpdateExpenseService : IPotScopedDependency
{
    Task<EnrichedResult<Output>> UpdateExpenseAsync(Input input, CancellationToken cancellationToken);
}
