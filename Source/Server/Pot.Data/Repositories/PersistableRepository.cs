using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Pot.Data.Entities;

namespace Pot.Data.Repositories;

internal abstract class PersistableRepository : RepositoryBase, IPersistableRepository
{
    protected PersistableRepository(PotDbContext dbContext)
        : base(dbContext)
    {
    }

    public EntityEntry<TEntity> Add<TEntity>(TEntity entity) where TEntity : EntityBase
    {
        return _dbContext.Add(entity);
    }

    public EntityEntry<TEntity> Update<TEntity>(TEntity entity) where TEntity : EntityBase
    {
        return _dbContext.Update(entity);
    }

    public EntityEntry<TEntity> Delete<TEntity>(TEntity entity) where TEntity : EntityBase
    {
        return _dbContext.Remove(entity);
    }

    public int Save()
    {
        return _dbContext.SaveChanges();
    }

    public Task<int> SaveAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<int> AddAndSaveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken) where TEntity : EntityBase
    {
        Add(entity);

        return SaveAsync(cancellationToken);
    }

    public Task<int> UpdateAndSaveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken) where TEntity : EntityBase
    {
        Update(entity);

        return SaveAsync(cancellationToken);
    }

    /// <summary>
    /// Loads the entities matching the query and marks them for deletion.
    /// </summary>
    /// <remarks>
    /// Must run inside a tracking scope. The rows are loaded rather than removed in a single database statement,
    /// so the change tracker can order the deletes against the foreign keys, which the ordered sweeps depend on.
    /// </remarks>
    /// <param name="query">The rows to remove.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of rows marked for deletion.</returns>
    protected async Task<int> DeleteWhereAsync<TEntity>(IQueryable<TEntity> query, CancellationToken cancellationToken)
        where TEntity : EntityBase
    {
        var entities = await query.ToListAsync(cancellationToken);

        foreach (var entity in entities)
        {
            Delete(entity);
        }

        return entities.Count;
    }
}
