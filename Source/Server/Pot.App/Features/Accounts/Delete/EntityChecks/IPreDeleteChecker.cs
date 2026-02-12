using Pot.App.Errors;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accounts.Delete.EntityChecks;

internal interface IPreDeleteChecker : IPotScopedDependency
{
    Task<ApiDetailError?> CanDeleteAsync(Guid accountId, CancellationToken cancellationToken);
}
