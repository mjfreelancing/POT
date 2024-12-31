using Pot.AspNetCore.Endpoints.Accounts.Import.Models;

namespace Pot.AspNetCore.Endpoints.Accounts.Import.Repository
{
    public interface IAccountImportRepository
    {
        Task ImportAccountsAsync(AccountImport[] accounts, CancellationToken cancellationToken);
    }
}
