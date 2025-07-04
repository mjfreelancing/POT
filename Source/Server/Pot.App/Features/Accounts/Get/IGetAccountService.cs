using AllOverIt.Patterns.Result;
using Pot.App.Features.Accounts.Get.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accounts.Get;

public interface IGetAccountService : IPotScopedDependency
{
    Task<EnrichedResult<Output>> GetAccountWithLinkedCountsAsync(Guid accountId, CancellationToken cancellationToken);
}
