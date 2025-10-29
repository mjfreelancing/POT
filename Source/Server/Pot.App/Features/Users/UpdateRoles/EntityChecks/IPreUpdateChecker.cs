using Pot.App.Errors;
using Pot.App.Features.Users.UpdateRoles.Models;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Users.UpdateRoles.EntityChecks;

public interface IPreUpdateChecker : IPotScopedDependency
{
    Task<ProblemDetailsError?> CanSaveAsync(Input input, UserEntity userToUpdate, CancellationToken cancellationToken);
}
