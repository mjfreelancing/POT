using Pot.App.Concerns.DependencyInjection;
using Pot.App.Errors;
using Pot.App.Features.Accounts.Update.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Accounts.Update.EntityChecks;

public interface IPreUpdateChecker : IPotScopedDependency
{
    Task<ProblemDetailsError?> CanSaveAsync(Input input, AccountEntity accountToUpdate, CancellationToken cancellationToken);
}
