using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;
using Pot.Shared;

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
}