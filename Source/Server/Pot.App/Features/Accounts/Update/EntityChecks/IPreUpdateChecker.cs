using Pot.App.Errors;
using Pot.App.Features.Accounts.Update.Models;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accounts.Update.EntityChecks;

public interface IPreUpdateChecker : IPotScopedDependency
{
    Task<ApiDetailError?> CanSaveAsync(Input input, AccountEntity accountToUpdate, CancellationToken cancellationToken);
}
