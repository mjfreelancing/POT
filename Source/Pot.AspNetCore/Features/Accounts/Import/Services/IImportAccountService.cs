using AllOverIt.Patterns.Result;
using Pot.AspNetCore.Concerns.DependencyInjection;
using Pot.AspNetCore.Features.Accounts.Import.Models;

namespace Pot.AspNetCore.Features.Accounts.Import.Services;

public interface IImportAccountService : IPotScopedDependency
{
    Task<EnrichedResult<ImportSummary>> ImportAccountsAsync(IEnumerable<AccountCsvRow> csvRows, CancellationToken cancellationToken);
}
