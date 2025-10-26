using AllOverIt.Assertion;
using Microsoft.EntityFrameworkCore.Storage;

namespace Pot.Data;

internal sealed class PotTransactionFactory : IPotTransactionFactory
{
    private readonly PotDbContext _dbContext;

    public PotTransactionFactory(PotDbContext dbContext)
    {
        _dbContext = dbContext.WhenNotNull();
    }

    public Task<IDbContextTransaction> CreateTransactionAsync(CancellationToken cancellationToken)
    {
        return _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }
}
