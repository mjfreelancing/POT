using Pot.AspNetCore.Features.Accounts.Import.Models;

namespace Pot.AspNetCore.Features.Accounts.Import.Services;

public interface IImportAccountService
{
    Task<ImportSummary> ImportAccountsAsync(AccountForImport[] accounts, bool overwrite, CancellationToken cancellationToken);
}
