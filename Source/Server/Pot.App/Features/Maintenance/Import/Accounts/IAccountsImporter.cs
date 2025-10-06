using Pot.App.Features.Maintenance.Import.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Maintenance.Import.Accounts;

public interface IAccountsImporter : IPotScopedDependency
{
    Task<int> ImportAsync(IEnumerable<IAccountCsvRow> csvRows, CancellationToken cancellationToken);
}
