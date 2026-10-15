using Pot.Data.Entities;
using Pot.Shared.Enumerations;

namespace Pot.Data.Repositories.Roles;

public interface IRoleRepository : IRepositoryBase
{
    IQueryable<RoleEntity> Roles { get; }

    Task<RoleEntity> GetByNameAsync(Role role, CancellationToken cancellationToken);
    Task<List<RoleEntity>> GetRolesAsync(Guid[] roleIds, CancellationToken cancellationToken);

    /// <summary>
    /// Resolves the roles granted to a user, and optionally each role's permissions.
    /// </summary>
    /// <remarks>
    /// Cross-tenant by design: permission resolution runs during authorization, before the user context
    /// middleware has established which site the caller is acting in, so the caller's own row is resolved by id
    /// with no site predicate. Roles and their permissions are global reference data rather than tenant data.
    /// </remarks>
    /// <param name="userRowId">The user whose grants are being resolved.</param>
    /// <param name="includePermissions">Whether to load each role's permissions.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The roles granted to the user.</returns>
    Task<List<RoleEntity>> GetRolesForUserAsync(Guid userRowId, bool includePermissions, CancellationToken cancellationToken);
}
