using Pot.AspNetCore.Concerns.DependencyInjection;
using Pot.Data.Entities;

namespace Pot.AspNetCore.Features.Accounts.Create.Services.PreSave;

public interface IPreCreateChecker : IPotScopedDependency
{
    Task<OutputState?> CanSaveAsync(AccountEntity accountToCreate, CancellationToken cancellationToken);
}
