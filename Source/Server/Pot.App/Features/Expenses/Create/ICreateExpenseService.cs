using AllOverIt.Patterns.Result;
using Pot.App.Features.Expenses.Create.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Expenses.Create;

public interface ICreateExpenseService : IPotScopedDependency
{
    Task<EnrichedResult<Output>> CreateExpenseAsync(Input input, CancellationToken cancellationToken);
}
