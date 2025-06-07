using AllOverIt.Patterns.Result;
using Pot.App.Concerns.DependencyInjection;
using Pot.App.Features.Accounts.Create.Models;

namespace Pot.App.Features.Accounts.Create;

public interface ICreateAccountService : IPotScopedDependency
{
    Task<EnrichedResult<Output>> CreateAccountAsync(Input input, CancellationToken cancellationToken);
}
