using AllOverIt.Patterns.Result;
using Pot.App.Features.Accounts.Create.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accounts.Create;

public interface ICreateAccountService : IPotScopedDependency
{
    Task<EnrichedResult<Output>> CreateAccountAsync(Input input, CancellationToken cancellationToken);
}
