using Pot.Data.Entities;

namespace Pot.Data.Exceptions;

public sealed class EntityConflictException<TEntity> : DatabaseException
    where TEntity : EntityBase
{
    public EntityConflictException(Guid rowId)
        : base(typeof(TEntity), $"A conflict occurred for entity '{rowId}'.")
    {
    }
}
