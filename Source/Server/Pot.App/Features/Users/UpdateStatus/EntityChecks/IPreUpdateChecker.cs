using Pot.App.Errors;
using Pot.App.Features.Users.UpdateStatus.Models;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Users.UpdateStatus.EntityChecks;

public interface IPreUpdateChecker : IPotScopedDependency
{
    Task<ApiDetailError?> CanSaveAsync(Input input, UserEntity userToUpdate, CancellationToken cancellationToken);
}
