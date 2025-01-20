using AllOverIt.Assertion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Pot.Data.Entities;
using System.Linq.Expressions;

namespace Pot.Data.Repositories;

internal abstract class GenericRepository<TDbContext, TEntity> : IGenericRepository<TDbContext, TEntity>
    where TEntity : EntityBase
    where TDbContext : DbContextBase
{
    public TDbContext DbContext { get; private set; }

    protected GenericRepository(TDbContext dbContext)
    {
        DbContext = dbContext.WhenNotNull();
    }

    public IQueryable<TEntity> GetAll()
    {
        return DbContext.Set<TEntity>();
    }

    public IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
    {
        return DbContext.Set<TEntity>().Where(predicate);
    }

    public Task<TEntity> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return DbContext.Set<TEntity>().SingleAsync(entity => entity.Id == id, cancellationToken);
    }

    public ValueTask<TEntity?> GetByIdOrDefaultAsync(int id, CancellationToken cancellationToken)
    {
        return DbContext.Set<TEntity>().FindAsync([id], cancellationToken);
    }

    public Task<TEntity> GetByRowIdAsync(Guid rowId, CancellationToken cancellationToken)
    {
        return DbContext.Set<TEntity>().SingleAsync(entity => entity.RowId == rowId, cancellationToken);
    }

    public Task<TEntity?> GetByRowIdOrDefaultAsync(Guid rowId, CancellationToken cancellationToken)
    {
        return DbContext.Set<TEntity>().SingleOrDefaultAsync(entity => entity.RowId == rowId, cancellationToken);
    }

    public EntityEntry<TEntity> Add(TEntity entity)
    {
        _ = entity.WhenNotNull();

        return DbContext.Set<TEntity>().Add(entity);
    }

    public ValueTask<EntityEntry<TEntity>> AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        _ = entity.WhenNotNull();

        return DbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    public EntityEntry<TEntity> Update(TEntity entity)
    {
        _ = entity.WhenNotNull();

        return DbContext.Set<TEntity>().Update(entity);
    }

    public virtual int Save()
    {
        return DbContext.SaveChanges();
    }

    public virtual Task<int> SaveAsync(CancellationToken cancellationToken)
    {
        return DbContext.SaveChangesAsync(cancellationToken);
    }
}
