using Pot.AspNetCore.Features.Accounts.Import.Models;

namespace Pot.AspNetCore.Features.Accounts.Import.Repository
{
    public interface IAccountImportRepository
    {
        Task ImportAccountsAsync(AccountImport[] accounts, CancellationToken cancellationToken);
    }
}
