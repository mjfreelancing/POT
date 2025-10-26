using Pot.App.Features.Users.GetAll.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Users.GetAll;

public interface IGetAllUsersService : IPotScopedDependency
{
    Task<List<Output>> GetAllUsersAsync(CancellationToken cancellationToken);
}
