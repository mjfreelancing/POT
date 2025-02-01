using AllOverIt.Assertion;
using Microsoft.EntityFrameworkCore;

namespace Pot.Data.UnitOfWork;

public abstract class UnitOfWork<TDbContext> : IUnitOfWork where TDbContext : DbContext
{
    private bool _disposed;

    protected TDbContext DbContext { get; private set; }

    public UnitOfWork(TDbContext dbContext)
    {
        DbContext = dbContext.WhenNotNull();
    }

    public void Dispose()
    {
        Dispose(true);
    }

    public virtual int Save()
    {
        return DbContext.SaveChanges();
    }

    public virtual Task<int> SaveAsync(CancellationToken cancellationToken)
    {
        return DbContext.SaveChangesAsync(cancellationToken);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                DbContext.Dispose();
            }

            _disposed = true;
        }
    }
}