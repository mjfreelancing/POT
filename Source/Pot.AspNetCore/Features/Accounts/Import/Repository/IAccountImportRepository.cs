using Pot.AspNetCore.Features.Accounts.Import.Models;

namespace Pot.AspNetCore.Features.Accounts.Import.Repository;

public interface IAccountImportRepository
{
    Task<ImportSummary> ImportAccountsAsync(AccountForImport[] accounts, bool overwrite, CancellationToken cancellationToken);
}
