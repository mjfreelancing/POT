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

    public IDisposable WithTracking() => _dbContext.WithAutoTracking();

    public IQueryable<TEntity> Set<TEntity>() where TEntity : EntityBase
    {
        return _dbContext.Set<TEntity>();
    }

    public EntityEntry GetEntry<TEntity>(TEntity entity) where TEntity : EntityBase
    {
        return _dbContext.Entry(entity);
    }

    public ValueTask<TEntity?> GetByPrimaryKeyAsync<TEntity, TKey>(TKey id, CancellationToken cancellationToken) where TEntity : EntityBase
    {
        return GetByPrimaryKeyAsync<TEntity>([id], cancellationToken);
    }

    public ValueTask<TEntity?> GetByPrimaryKeyAsync<TEntity>(object?[]? values, CancellationToken cancellationToken) where TEntity : EntityBase
    {
        return _dbContext.Set<TEntity>().FindAsync(values, cancellationToken);
    }
}
