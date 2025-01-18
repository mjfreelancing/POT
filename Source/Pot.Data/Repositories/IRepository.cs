using Pot.Data.Entities;

namespace Pot.Data.Repositories;

public interface IRepository<TEntity> where TEntity : EntityBase
{
    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken);
    Task<TEntity> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<TEntity?> GetByIdOrDefaultAsync(int id, CancellationToken cancellationToken);
    Task CreateAsync(TEntity entity, CancellationToken cancellationToken);
    Task UpdateRangeAsync(TEntity[] entities, CancellationToken cancellationToken);
    Task DeleteByIdAsync(int id, CancellationToken cancellationToken);
}
