using Microsoft.EntityFrameworkCore.ChangeTracking;
using Pot.Data.Entities;
using System.Linq.Expressions;

namespace Pot.Data.Repositories;

public interface IGenericRepository<TDbContext, TEntity>
    where TEntity : EntityBase
    where TDbContext : DbContextBase
{
    IDisposable WithTracking();


    // IQueryable

    // =======================
    IQueryable<TEntity> AsQueryable();
    IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate);

    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
    Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
    Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);

    // Get data
    // =======================
    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken);
    ValueTask<TEntity?> GetByPrimaryKeyAsync<TKey>(TKey id, CancellationToken cancellationToken);
    ValueTask<TEntity?> GetByPrimaryKeyAsync(object?[]? values, CancellationToken cancellationToken);


    // Add data
    // =======================
    EntityEntry<TEntity> Add(TEntity entity);

    // Only use when value generators need to async communicate with the database.
    ValueTask<EntityEntry<TEntity>> AddAsync(TEntity entity, CancellationToken cancellationToken);



    // Update data
    // =======================
    EntityEntry<TEntity> Update(TEntity entity);

    // Delete data
    // =======================
    EntityEntry<TEntity> Delete(TEntity entity);





    // experimental below here
    // =======================

    //Task<List<TEntity>> FindWithPropertyValue<TProperty>(string propertyName, TProperty? value, CancellationToken cancellationToken);

    //Task<TEntity> GetByIdAsync(int id, CancellationToken cancellationToken);

    //Task<TEntity> GetByRowIdAsync(Guid rowId, CancellationToken cancellationToken);
    //Task<TEntity?> GetByRowIdOrDefaultAsync(Guid rowId, CancellationToken cancellationToken);

    //IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
}
