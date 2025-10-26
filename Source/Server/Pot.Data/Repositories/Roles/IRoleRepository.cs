using Pot.Data.Entities;
using Pot.Shared;

namespace Pot.Data.Repositories.Roles;

public interface IRoleRepository : IRepositoryBase
{
    IQueryable<RoleEntity> Roles { get; }

    Task<RoleEntity> GetByNameAsync(Role role, CancellationToken cancellationToken);
}
