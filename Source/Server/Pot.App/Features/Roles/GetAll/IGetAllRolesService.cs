using Pot.App.Features.Roles.GetAll.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Roles.GetAll;

public interface IGetAllRolesService : IPotScopedDependency
{
    Task<List<Output>> GetAllRolesAsync(CancellationToken cancellationToken);
}
