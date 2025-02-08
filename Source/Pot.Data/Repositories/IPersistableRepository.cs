using Pot.Data.Entities;

namespace Pot.Data.Repositories;

public interface IPersistableRepository<TEntity>
    where TEntity : EntityBase
{
    int Save();
    Task<int> SaveAsync(CancellationToken cancellationToken);
    Task<int> AddAndSaveAsync(TEntity entity, CancellationToken cancellationToken);
}
