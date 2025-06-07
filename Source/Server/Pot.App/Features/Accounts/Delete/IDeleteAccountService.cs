using Pot.App.Concerns.DependencyInjection;

namespace Pot.App.Features.Accounts.Delete;

public interface IDeleteAccountService : IPotScopedDependency
{
    Task<bool> DeleteAccountAsync(Guid id, CancellationToken cancellationToken);
}
