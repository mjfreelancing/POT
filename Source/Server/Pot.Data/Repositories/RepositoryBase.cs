using AllOverIt.Assertion;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Pot.Data.Entities;
using Pot.Data.Extensions;

namespace Pot.Data.Repositories;

internal abstract class RepositoryBase : IRepositoryBase
{
    protected readonly PotDbContext _dbContext;

    public RepositoryBase(PotDbContext dbContext)
    {
        _dbContext = dbContext.WhenNotNull();
    }

    public IDisposable WithTracking()
    {
        return _dbContext.WithAutoTracking();
    }

    public EntityEntry GetEntry<TEntity>(TEntity entity) where TEntity : EntityBase
    {
        return _dbContext.Entry(entity);
    }

    /// <summary>
    /// A queryable for an entity type, for use inside a repository implementation.
    /// </summary>
    /// <remarks>
    /// Kept off <see cref="IRepositoryBase" /> on purpose: a queryable on the interface would let any caller write
    /// a lookup with no site predicate, which is what the site-scoped repository methods exist to prevent. A
    /// caller outside this assembly resolves through an intent-named method instead.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <returns>A queryable for the entity type.</returns>
    public IQueryable<TEntity> Set<TEntity>() where TEntity : EntityBase
    {
        return _dbContext.Set<TEntity>();
    }
}
