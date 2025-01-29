using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Pot.AspNetCore.Features.Accounts.Import.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;

namespace Pot.AspNetCore.Features.Accounts.Import.Services;

internal sealed class ImportAccountService : IImportAccountService
{
    private sealed record AccountKey(string Bsb, string Number);

    private readonly IAccountRepository _accountRepository;
    private readonly ILogger _logger;

    public ImportAccountService(IAccountRepository accountRepository, ILogger<ImportAccountService> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    // The account BSB / Number is used for identifying a record that can be overwritten
    public async Task<ImportSummary> ImportAccountsAsync(AccountForImport[] accountsForImport, bool overwrite, CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { overwrite });

        using (_accountRepository.WithTracking())
        {
            //var importAccountKeys = accountsForImport.ToDictionary(account => new AccountKey(account.Bsb, account.Number));

            var imported = 0;
            var updated = 0;

            foreach (var import in accountsForImport)
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
                Skipped = accountsForImport.Length - imported - updated,
                Imported = imported,
                Updated = updated,
                Total = accountsForImport.Length
            };
        }
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
