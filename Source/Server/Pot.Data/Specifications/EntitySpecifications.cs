using AllOverIt.Patterns.Specification;
using Pot.Data.Entities;

namespace Pot.Data.Specifications;

public static class EntitySpecifications
{
    public static ILinqSpecification<TEntity> IsSameId<TEntity>(Guid rowId) where TEntity : EntityBase
    {
        return LinqSpecification<TEntity>.Create(entity => entity.RowId == rowId);
    }
}
