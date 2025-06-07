using AllOverIt.Patterns.Result;
using Pot.App.Concerns.DependencyInjection;
using Pot.App.Features.Accounts.Update.Models;

namespace Pot.App.Features.Accounts.Update;

public interface IUpdateAccountService : IPotScopedDependency
{
    Task<EnrichedResult<Output>> UpdateAccountAsync(Input input, CancellationToken cancellationToken);
}
