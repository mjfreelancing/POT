using Pot.App.Concerns.DependencyInjection;
using Pot.App.Errors;
using Pot.Data.Entities;

namespace Pot.App.Features.Accounts.Create.EntityChecks;

internal interface IPreCreateChecker : IPotScopedDependency
{
    Task<ProblemDetailsError?> CanSaveAsync(AccountEntity accountToCreate, CancellationToken cancellationToken);
}
