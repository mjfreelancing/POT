using Pot.Data.Entities;

namespace Pot.Data.Exceptions;

public sealed class EntityNotFoundException<TEntity> : DatabaseException
    where TEntity : EntityBase
{
    public EntityNotFoundException()
        : base(typeof(TEntity), "Unable to find entity.")
    {
    }

    public EntityNotFoundException(Guid rowId)
        : base(typeof(TEntity), $"Unable to find entity '{rowId}'.")
    {
    }
}
