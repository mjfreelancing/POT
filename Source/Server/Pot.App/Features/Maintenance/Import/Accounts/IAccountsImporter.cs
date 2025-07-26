using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Maintenance.Import.Accounts;

public interface IAccountsImporter : IPotScopedDependency
{
    Task<int> ImportAsync(Stream zipStream, CancellationToken cancellationToken);
}
