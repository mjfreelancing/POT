using AllOverIt.Patterns.Result;
using Pot.App.Concerns.DependencyInjection;
using Pot.App.Features.Expenses.Create.Models;

namespace Pot.App.Features.Expenses.Create;

public interface ICreateExpenseService : IPotScopedDependency
{
    Task<EnrichedResult<Output>> CreateExpenseAsync(Input input, CancellationToken cancellationToken);
}
