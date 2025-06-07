using Pot.App.Concerns.DependencyInjection;
using Pot.App.Errors;

namespace Pot.App.Features.Accounts.Delete.EntityChecks;

internal interface IPreCreateChecker : IPotScopedDependency
{
    Task<ProblemDetailsError?> CanSaveAsync(Guid accountId, CancellationToken cancellationToken);
}
