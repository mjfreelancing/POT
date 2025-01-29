using AllOverIt.Assertion;
using AllOverIt.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Pot.Data.Entities;
using Pot.Data.Extensions;
using System.Linq.Expressions;

namespace Pot.Data.Repositories;

// TODO: AOI
internal abstract class GenericRepository<TDbContext, TEntity> : IGenericRepository<TDbContext, TEntity>
    where TEntity : EntityBase
    where TDbContext : DbContextBase
{
    // Note: Not awaiting in these methods for performance reasons. Exceptions will be reported at the point of awaiting.

    public TDbContext DbContext { get; private set; }

    protected GenericRepository(TDbContext dbContext)
    {
        DbContext = dbContext.WhenNotNull();
    }

    public IDisposable WithTracking() => DbContext.WithAutoTracking();

    // IQueryable
    public IQueryable<TEntity> Query()
    {
        return DbContext.Set<TEntity>();
    }

    public IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
    {
        return DbContext.Set<TEntity>().Where(predicate);
    }

    // Get data
    // =======================
    public Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken)
    {
        return Query().ToListAsync(cancellationToken);
    }

    public ValueTask<TEntity?> GetByPrimaryKeyAsync<TKey>(TKey id, CancellationToken cancellationToken)
    {
        return GetByPrimaryKeyAsync([id], cancellationToken);
    }

    public ValueTask<TEntity?> GetByPrimaryKeyAsync(object?[]? values, CancellationToken cancellationToken)
    {
        return DbContext.Set<TEntity>().FindAsync(values, cancellationToken);
    }

    // Add data
    // =======================
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

    public Task<int> AddAndSaveAsync(TEntity entity, CancellationToken cancellationToken)
    {
        Add(entity);

        return SaveAsync(cancellationToken);
    }

    // Update data
    // =======================
    public EntityEntry<TEntity> Update(TEntity entity)
    {
        _ = entity.WhenNotNull();

        return DbContext.Set<TEntity>().Update(entity);
    }

    // Save data
    // =======================
    public virtual int Save()
    {
        return DbContext.SaveChanges();
    }

    public virtual Task<int> SaveAsync(CancellationToken cancellationToken)
    {
        return DbContext.SaveChangesAsync(cancellationToken);
    }








    // experimental below here
    // =======================




    //public Task<List<TEntity>> FindByColumn<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, TProperty value, CancellationToken cancellationToken)
    //{
    //    var comparison = ExpressionHelpers.GetEqualExpression(propertyExpression, value);
    //    var parameter = propertyExpression.Parameters[0];
    //    var predicate = Expression.Lambda<Func<TEntity, bool>>(comparison, parameter);

    //    return DbContext.Set<TEntity>().Where(predicate).ToListAsync(cancellationToken);
    //}

    //public Task<TEntity> GetByColumn<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, TProperty value, CancellationToken cancellationToken)
    //{
    //    var comparison = ExpressionHelpers.GetEqualExpression(propertyExpression, value);
    //    var parameter = propertyExpression.Parameters[0];
    //    var predicate = Expression.Lambda<Func<TEntity, bool>>(comparison, parameter);

    //    return DbContext.Set<TEntity>().SingleAsync(predicate, cancellationToken);
    //}


    //public Task<TEntity> GetByIdAsync(int id, CancellationToken cancellationToken)
    //{
    //    return DbContext.Set<TEntity>().SingleAsync(entity => entity.Id == id, cancellationToken);
    //}

    //public Task<TEntity> GetByRowIdAsync(Guid rowId, CancellationToken cancellationToken)
    //{
    //    return DbContext.Set<TEntity>().SingleAsync(entity => entity.RowId == rowId, cancellationToken);
    //}

    //public Task<TEntity?> GetByRowIdOrDefaultAsync(Guid rowId, CancellationToken cancellationToken)
    //{
    //    return DbContext.Set<TEntity>().SingleOrDefaultAsync(entity => entity.RowId == rowId, cancellationToken);
    //}


    //public Task<List<TEntity>> FindWithPropertyValue<TProperty>(string propertyName, TProperty? value, CancellationToken cancellationToken)
    //{
    //    var propertyExpression = PropertyHelper.CreatePropertyGetterExpressionLambda<TEntity, TProperty>(propertyName);
    //    var comparison = ExpressionHelpers.GetEqualExpression(propertyExpression, value);
    //    var parameter = propertyExpression.Parameters[0];
    //    var predicate = Expression.Lambda<Func<TEntity, bool>>(comparison, parameter);

    //    return DbContext.Set<TEntity>().Where(predicate).ToListAsync(cancellationToken);
    //}


    //public IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
    //{
    //    return DbContext.Set<TEntity>().Where(predicate);
    //}
}


internal static class ExpressionHelpers
{
    // TODO: AOI - extend to other comparison expressions - ? useful
    public static BinaryExpression GetEqualExpression<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, TProperty? value)
        where TEntity : EntityBase
    {
        // Parameterizing the value rather than a constant as it allows EF to cache the queries more efficiently
        var parameterizedValue = ExpressionUtils.CreateParameterizedValue(value);

        // The Body is 'entity.Name' in 'entity => entity.Name'
        return Expression.Equal(propertyExpression.Body, parameterizedValue);
    }

    public static BinaryExpression GetEqualExpression<TEntity, TProperty>(string propertyName, TProperty? value)
        where TEntity : EntityBase
    {
        // Create the parameter expression (entity)
        var parameter = Expression.Parameter(typeof(TEntity), "entity");

        // Create the property access (entity.PropertyName)
        var property = Expression.Property(parameter, propertyName);

        // Cast the property to TProperty (if necessary)
        var propertyAccess = Expression.Convert(property, typeof(TProperty));

        // Parameterizing the value rather than a constant as it allows EF to cache the queries more efficiently
        var parameterizedValue = ExpressionUtils.CreateParameterizedValue(value);

        return Expression.Equal(propertyAccess, parameterizedValue);
    }
}



// TODO: Now in AOI
public static class PropertyHelper
{
    public static Expression<Func<TEntity, TProperty>> CreatePropertyGetterExpressionLambda<TEntity, TProperty>(string propertyName)
    {
        // Create the parameter expression (entity)
        var parameter = Expression.Parameter(typeof(TEntity), "entity");

        // Create the property access (entity.PropertyName)
        var property = Expression.Property(parameter, propertyName);

        // Cast the property to TProperty (if necessary)
        var propertyAccess = Expression.Convert(property, typeof(TProperty));

        // Create and return the lambda expression
        return Expression.Lambda<Func<TEntity, TProperty>>(propertyAccess, parameter);
    }
}