using Pot.Data.Entities;

namespace Pot.Data.Repositories;

public interface IRepository<TEntity> where TEntity : EntityBase
{
    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken);
    Task<TEntity> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<TEntity?> GetByIdOrDefaultAsync(int id, CancellationToken cancellationToken);
    Task<int> CreateAsync(TEntity entity, CancellationToken cancellationToken);
    Task<int> UpdateRangeAsync(TEntity[] entities, CancellationToken cancellationToken);
    Task<int> DeleteByIdAsync(int id, CancellationToken cancellationToken);
}
