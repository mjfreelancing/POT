using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;
using Pot.Data.Extensions;
using Pot.Data.Specifications;

namespace Pot.Data.Extensions;

internal static class QueryableExtensions
{
    public static Task<TEntity?> SingleOrDefaultAsync<TEntity>(this IQueryable<TEntity> queryable, Guid id, CancellationToken cancellationToken) where TEntity : EntityBase
    {
        return queryable.SingleOrDefaultAsync(EntitySpecifications.IsSameId<TEntity>(id).Expression, cancellationToken);
    }

    public static Task<TEntity> SingleAsync<TEntity>(this IQueryable<TEntity> queryable, Guid id, CancellationToken cancellationToken) where TEntity : EntityBase
    {
        return queryable.SingleAsync(EntitySpecifications.IsSameId<TEntity>(id).Expression, cancellationToken);
    }

    public static Task<bool> AnyAsync<TEntity>(this IQueryable<TEntity> queryable, Guid id, CancellationToken cancellationToken) where TEntity : EntityBase
    {
        return queryable.AnyAsync(EntitySpecifications.IsSameId<TEntity>(id).Expression, cancellationToken);
    }
}
