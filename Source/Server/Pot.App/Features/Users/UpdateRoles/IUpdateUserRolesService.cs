using AllOverIt.Patterns.Result;
using Pot.App.Features.Users.UpdateRoles.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Users.UpdateRoles;

public interface IUpdateUserRolesService : IPotScopedDependency
{
    Task<EnrichedResult<Output>> UpdateUserRolesAsync(Input input, CancellationToken cancellationToken);
}
