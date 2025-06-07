using AllOverIt.Patterns.Result;
using Pot.App.Concerns.DependencyInjection;

namespace Pot.App.Features.Accounts.Delete;

public interface IDeleteAccountService : IPotScopedDependency
{
    Task<EnrichedResult<bool>> DeleteAccountAsync(Guid accountId, CancellationToken cancellationToken);
}
