using Pot.AspNetCore.Concerns.DependencyInjection;
using Pot.Data.Entities;

namespace Pot.AspNetCore.Features.Accounts.Update.Services.PreSave;

public interface IPreUpdateChecker : IPotScopedDependency
{
    Task<OutputState?> CanSaveAsync(Request request, AccountEntity accountToUpdate, CancellationToken cancellationToken);
}
