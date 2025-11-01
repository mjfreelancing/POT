using Pot.Data.Entities;
using Pot.Shared.Enumerations;

namespace Pot.Data.Repositories.Roles;

public interface IRoleRepository : IRepositoryBase
{
    IQueryable<RoleEntity> Roles { get; }

    Task<RoleEntity> GetByNameAsync(Role role, CancellationToken cancellationToken);
    Task<List<RoleEntity>> GetRolesAsync(Guid[] roleIds, CancellationToken cancellationToken);
    Task<List<RoleEntity>> GetRolesForUserAsync(Guid userRowId, bool includePermissions, CancellationToken cancellationToken);
}
