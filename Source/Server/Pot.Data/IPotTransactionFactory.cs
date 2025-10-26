using Microsoft.EntityFrameworkCore.Storage;
using Pot.Shared.DependencyInjection;

namespace Pot.Data;

public interface IPotTransactionFactory : IPotScopedDependency
{
    Task<IDbContextTransaction> CreateTransactionAsync(CancellationToken cancellationToken);
}
