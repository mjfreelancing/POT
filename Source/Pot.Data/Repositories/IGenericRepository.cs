using Microsoft.EntityFrameworkCore.ChangeTracking;
using Pot.Data.Entities;
using System.Linq.Expressions;

namespace Pot.Data.Repositories;

public interface IGenericRepository<TDbContext, TEntity>
    where TEntity : EntityBase
    where TDbContext : DbContextBase
{
    TDbContext DbContext { get; }

    IQueryable<TEntity> GetAll();
    IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);

    Task<TEntity> GetByIdAsync(int id, CancellationToken cancellationToken);
    ValueTask<TEntity?> GetByIdOrDefaultAsync(int id, CancellationToken cancellationToken);

    Task<TEntity> GetByRowIdAsync(Guid rowId, CancellationToken cancellationToken);
    Task<TEntity?> GetByRowIdOrDefaultAsync(Guid rowId, CancellationToken cancellationToken);

    EntityEntry<TEntity> Add(TEntity entity);

    // Only use when value generators need to async communicate with the database.
    ValueTask<EntityEntry<TEntity>> AddAsync(TEntity entity, CancellationToken cancellationToken);

    EntityEntry<TEntity> Update(TEntity entity);

    int Save();
    Task<int> SaveAsync(CancellationToken cancellationToken);

}
