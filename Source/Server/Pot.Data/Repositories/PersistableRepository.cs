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
}
