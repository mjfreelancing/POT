using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.Data.Repositories;

public interface IRepositoryBase : IPotScopedDependency
{
    // On IRepositoryBase just in case a read entity needs to be attached to another processed by IPersistableRepository
    IDisposable WithTracking();

    IQueryable<TEntity> Set<TEntity>() where TEntity : EntityBase;

    ValueTask<TEntity?> GetByPrimaryKeyAsync<TEntity, TKey>(TKey id, CancellationToken cancellationToken) where TEntity : EntityBase;
    ValueTask<TEntity?> GetByPrimaryKeyAsync<TEntity>(object?[]? values, CancellationToken cancellationToken) where TEntity : EntityBase;
}
