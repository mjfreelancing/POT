using Pot.App.Errors;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accounts.Delete.EntityChecks;

internal interface IPreDeleteChecker : IPotScopedDependency
{
    Task<ProblemDetailsError?> CanDeleteAsync(Guid accountId, CancellationToken cancellationToken);
}
