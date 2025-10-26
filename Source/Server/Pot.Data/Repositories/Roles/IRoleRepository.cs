using Pot.Data.Entities;
using Pot.Shared;

namespace Pot.Data.Repositories.Roles;

public interface IRoleRepository : IRepositoryBase
{
    Task<RoleEntity> GetByNameAsync(Role role, CancellationToken cancellationToken);
}
