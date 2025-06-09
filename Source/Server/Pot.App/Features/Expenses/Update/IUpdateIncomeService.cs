using AllOverIt.Patterns.Result;
using Pot.App.Concerns.DependencyInjection;
using Pot.App.Features.Expenses.Update.Models;

namespace Pot.App.Features.Expenses.Update;

public interface IUpdateExpenseService : IPotScopedDependency
{
    Task<EnrichedResult<Output>> UpdateExpenseAsync(Input input, CancellationToken cancellationToken);
}
