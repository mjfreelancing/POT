using AllOverIt.Patterns.Result;
using Pot.App.Features.Accounts.Import.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accounts.Import;

public interface IImportAccountService : IPotScopedDependency
{
    Task<EnrichedResult<ImportSummary>> ImportAccountsAsync(IEnumerable<AccountCsvRow> csvRows, CancellationToken cancellationToken);
}
