using AllOverIt.Assertion;
using Pot.AspNetCore.Features.Accounts.Import.Models;
using Pot.Data.Entities;
using Pot.Data.Extensions;
using Pot.Data.Repositories.Accounts;

namespace Pot.AspNetCore.Features.Accounts.Import.Services;

internal sealed class ImportAccountService : IImportAccountService
{
    private sealed record AccountKey(string Bsb, string Number);

    private readonly IAccountRepository _accountRepository;

    public ImportAccountService(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _accountRepository.DbContext.WithTracking(true);
    }

    public async Task<ImportSummary> ImportAccountsAsync(AccountForImport[] accountsImport, bool overwrite, CancellationToken cancellationToken)
    {
        var importAccountKeys = accountsImport.ToDictionary(account => new AccountKey(account.Bsb, account.Number));

        var imported = 0;
        var updated = 0;

        foreach (var import in accountsImport)
        {
            var existing = await _accountRepository
                .GetAccountOrDefaultAsync(import.Bsb, import.Number, cancellationToken)
                .ConfigureAwait(false);

            if (existing is null)
            {
                AddAccountEntity(import);
                imported++;
            }
            else if (overwrite)
            {
                UpdateAccountEntity(existing, import);
                updated++;
            }
        }

        await _accountRepository
            .SaveAsync(cancellationToken)
            .ConfigureAwait(false);

        return new ImportSummary
        {
            Skipped = accountsImport.Length - imported - updated,
            Imported = imported,
            Updated = updated,
            Total = accountsImport.Length
        };
    }

    private void AddAccountEntity(AccountForImport import)
    {
        var newAccount = new AccountEntity
        {
            Bsb = import.Bsb,
            Number = import.Number,
            Description = import.Description,
            Balance = import.Balance,
            Reserved = import.Reserved,
            Allocated = 0.0d,
            DailyAccrual = 0.0d
        };

        _accountRepository.Add(newAccount);
    }

    private static void UpdateAccountEntity(AccountEntity entity, AccountForImport import)
    {
        entity.Bsb = import.Bsb;
        entity.Number = import.Number;
        entity.Description = import.Description;
        entity.Balance = import.Balance;
        entity.Reserved = import.Reserved;

        // Leave these at their current values
        // entity.Allocated
        // entity.DailyAccrual

        // Don't need to explicitly call _accountRepository.Update(entity). The entity will
        // be marked as modified if anything has changed.
    }
}
