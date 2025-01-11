namespace Pot.Data.Exceptions;

public abstract class DatabaseException : Exception
{
    public Type EntityType { get; }

    protected DatabaseException(Type entityType)
    {
        EntityType = entityType;
    }

    protected DatabaseException(Type entityType, string message)
        : base(message)
    {
        EntityType = entityType;
    }
}
