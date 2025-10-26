using Microsoft.EntityFrameworkCore.ChangeTracking;
using Pot.Data.Entities;

namespace Pot.Data.Repositories;

public interface IPersistableRepository
{
    EntityEntry<TEntity> Add<TEntity>(TEntity entity) where TEntity : EntityBase;
    EntityEntry<TEntity> Update<TEntity>(TEntity entity) where TEntity : EntityBase;
    EntityEntry<TEntity> Delete<TEntity>(TEntity entity) where TEntity : EntityBase;

    int Save();
    Task<int> SaveAsync(CancellationToken cancellationToken);
    Task<int> AddAndSaveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken) where TEntity : EntityBase;
    Task<int> UpdateAndSaveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken) where TEntity : EntityBase;
}
