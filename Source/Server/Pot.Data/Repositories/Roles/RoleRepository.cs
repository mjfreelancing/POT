using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;
using Pot.Shared.Enumerations;

namespace Pot.Data.Repositories.Roles;

internal sealed class RoleRepository : RepositoryBase, IRoleRepository
{
    public IQueryable<RoleEntity> Roles => _dbContext.Roles;

    public RoleRepository(PotDbContext dbContext)
        : base(dbContext)
    {
    }

    public Task<RoleEntity> GetByNameAsync(Role role, CancellationToken cancellationToken)
    {
        return Roles.SingleAsync(entity => entity.Name == role, cancellationToken);
    }

    public Task<List<RoleEntity>> GetRolesAsync(Guid[] roleIds, CancellationToken cancellationToken)
    {
        return _dbContext.Roles
            .Where(role => roleIds.Contains(role.RowId))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<RoleEntity>> GetRolesForUserAsync(Guid userRowId, bool includePermissions, CancellationToken cancellationToken)
    {
        var userQuery = _dbContext.Users.Where(user => user.RowId == userRowId);

        userQuery = includePermissions
            ? userQuery.Include(user => user.Roles).ThenInclude(role => role.Permissions)
            : userQuery.Include(user => user.Roles);

        var rolesQuery = userQuery.Select(user => user.Roles);

        var roles = await rolesQuery
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return [.. roles.SelectMany(role => role)];
    }
}