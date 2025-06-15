using Pot.App.Errors;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accounts.Delete.EntityChecks;

internal interface IPreCreateChecker : IPotScopedDependency
{
    Task<ProblemDetailsError?> CanSaveAsync(Guid accountId, CancellationToken cancellationToken);
}
