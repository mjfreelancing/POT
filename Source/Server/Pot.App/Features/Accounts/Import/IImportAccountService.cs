using AllOverIt.Patterns.Result;
using Pot.App.Concerns.DependencyInjection;
using Pot.App.Features.Accounts.Import.Models;

namespace Pot.App.Features.Accounts.Import;

public interface IImportAccountService : IPotScopedDependency
{
    Task<EnrichedResult<ImportSummary>> ImportAccountsAsync(IEnumerable<AccountCsvRow> csvRows, CancellationToken cancellationToken);
}
