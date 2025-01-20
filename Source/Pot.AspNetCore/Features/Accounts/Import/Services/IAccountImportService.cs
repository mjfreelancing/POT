using Pot.AspNetCore.Features.Accounts.Import.Models;

namespace Pot.AspNetCore.Features.Accounts.Import.Services;

public interface IAccountImportService
{
    Task<ImportSummary> ImportAccountsAsync(AccountForImport[] accounts, bool overwrite, CancellationToken cancellationToken);
}
