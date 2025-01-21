using Microsoft.EntityFrameworkCore.ChangeTracking;
using Pot.Data.Entities;

namespace Pot.Data.Repositories;

public interface IGenericRepository<TDbContext, TEntity>
    where TEntity : EntityBase
    where TDbContext : DbContextBase
{
    //Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken);
    //Task<TEntity> GetByIdAsync(int id, CancellationToken cancellationToken);
    //Task<TEntity?> GetByIdOrDefaultAsync(int id, CancellationToken cancellationToken);
    //Task<TEntity?> GetByRowIdOrDefaultAsync(Guid rowId, CancellationToken cancellationToken);
    //Task<int> CreateAsync(TEntity entity, CancellationToken cancellationToken);
    //Task<int> UpdateRangeAsync(TEntity[] entities, CancellationToken cancellationToken);
    //Task<int> DeleteByIdAsync(int id, CancellationToken cancellationToken);



    TDbContext DbContext { get; }

    // IQueryable
    // =======================
    IQueryable<TEntity> Get();


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

    Task<int> AddAndSaveAsync(TEntity entity, CancellationToken cancellationToken);


    // Update data
    // =======================
    EntityEntry<TEntity> Update(TEntity entity);


    // Save data
    // =======================
    int Save();
    Task<int> SaveAsync(CancellationToken cancellationToken);









    // experimental below here
    // =======================

    //Task<List<TEntity>> FindWithPropertyValue<TProperty>(string propertyName, TProperty? value, CancellationToken cancellationToken);

    //Task<TEntity> GetByIdAsync(int id, CancellationToken cancellationToken);

    //Task<TEntity> GetByRowIdAsync(Guid rowId, CancellationToken cancellationToken);
    //Task<TEntity?> GetByRowIdOrDefaultAsync(Guid rowId, CancellationToken cancellationToken);

    //IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
}
