namespace Pot.Data.Entities;

public interface IHasNextDue
{
    DateOnly NextDue { get; }
    DateOnly? EndDate { get; }
}