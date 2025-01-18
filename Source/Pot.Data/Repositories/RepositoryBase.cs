using AllOverIt.Assertion;
using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;
using Pot.Data.Exceptions;

namespace Pot.Data.Repositories;

internal abstract class RepositoryBase<TEntity> : IRepository<TEntity> where TEntity : EntityBase
{
    protected readonly IDbContextFactory<PotDbContext> _dbContextFactory;

    protected RepositoryBase(IDbContextFactory<PotDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory.WhenNotNull();
    }

    public virtual async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Set<TEntity>().ToListAsync(cancellationToken);
    }

    public virtual async Task<TEntity> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var entity = await dbContext.Set<TEntity>().FindAsync([id], cancellationToken);

        return entity is null
            ? throw new EntityNotFoundException<TEntity>()
            : entity;
    }

    public virtual async Task<TEntity?> GetByIdOrDefaultAsync(int id, CancellationToken cancellationToken)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Set<TEntity>().FindAsync([id], cancellationToken);
    }

    public virtual async Task<int> CreateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        _ = entity.WhenNotNull();

        using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        // AddAsync() should only be used for special cases where value generators need to access the database asynchronously
        dbContext.Set<TEntity>().Add(entity);

        return await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task<int> UpdateRangeAsync(TEntity[] entities, CancellationToken cancellationToken)
    {
        _ = entities.WhenNotNullOrEmpty();

        using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        dbContext.Set<TEntity>().UpdateRange(entities);

        return await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task<int> DeleteByIdAsync(int id, CancellationToken cancellationToken)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var entity = await GetByIdAsync(id, cancellationToken).ConfigureAwait(false);

        dbContext.Set<TEntity>().Remove(entity);

        return await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}

