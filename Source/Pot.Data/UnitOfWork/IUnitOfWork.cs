namespace Pot.Data.UnitOfWork;

public interface IUnitOfWork
{
    int Save();
    Task<int> SaveAsync(CancellationToken cancellationToken);
}
