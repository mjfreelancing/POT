using AllOverIt.Patterns.Result;
using Pot.App.Features.Accounts.Update.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accounts.Update;

public interface IUpdateAccountService : IPotScopedDependency
{
    Task<EnrichedResult<Output>> UpdateAccountAsync(Input input, CancellationToken cancellationToken);
}
