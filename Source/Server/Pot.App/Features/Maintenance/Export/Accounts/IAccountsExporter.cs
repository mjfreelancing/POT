using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Maintenance.Export.Accounts;

public interface IAccountsExporter : IPotScopedDependency
{
    Task<byte[]> ExportAllAsync(CancellationToken cancellationToken);
}
