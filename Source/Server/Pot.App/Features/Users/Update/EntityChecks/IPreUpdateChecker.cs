using Pot.App.Errors;
using Pot.App.Features.Users.Update.Models;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Users.Update.EntityChecks;

public interface IPreUpdateChecker : IPotScopedDependency
{
    Task<ProblemDetailsError?> CanSaveAsync(Input input, UserEntity userToUpdate, CancellationToken cancellationToken);
}
