using AllOverIt.Assertion;
using CsvHelper;
using Pot.App.Features.Maintenance.Import.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using System.Globalization;

namespace Pot.App.Features.Maintenance.Import;

internal sealed class AccountsImporter : IAccountsImporter
{
    private readonly IPersistableAccountRepository _accountRepository;

    public AccountsImporter(IPersistableAccountRepository accountRepository)
    {
        _accountRepository = accountRepository.WhenNotNull();
    }

    public async Task<int> ImportAsync(Stream zipStream, CancellationToken cancellationToken)
    {
        using StreamReader reader = new(zipStream);

        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        csv.Read();
        csv.ReadHeader();

        var csvRows = csv.GetRecords<AccountCsvRow>().ToList();

        using (_accountRepository.WithTracking())
        {
            foreach (var csvRow in csvRows)
            {
                await CreateOrUpdateAccountAsync(csvRow, cancellationToken).ConfigureAwait(false);
            }

            // Could throw UniqueConstraintException (or a related database exception),
            // resulting in a custom 422 ProblemDetails response.
            await _accountRepository
                .SaveAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        return csvRows.Count;
    }

    private async Task CreateOrUpdateAccountAsync(AccountCsvRow csvRow, CancellationToken cancellationToken)
    {
        var csvAccountId = csvRow.RowId;

        var accountEntity = await _accountRepository
            .GetAccountOrDefaultAsync(csvAccountId, cancellationToken)
            .ConfigureAwait(false);

        if (accountEntity is null)
        {
            CreateAccountEntity(csvRow);
        }
        else
        {
            UpdateExistingAccount(accountEntity, csvRow);
        }
    }

    private AccountEntity CreateAccountEntity(AccountCsvRow import)
    {
        var accountEntity = new AccountEntity
        {
            RowId = import.RowId,
            Bsb = import.Bsb,
            Number = import.Number,
            Description = import.Description,
            Balance = import.Balance,
            Reserved = import.Reserved,
            TotalExpenseAccrued = import.TotalExpenseAccrued,
            DailyExpenseAccrual = import.DailyExpenseAccrual
        };

        _accountRepository.Add(accountEntity);

        return accountEntity;
    }

    private static void UpdateExistingAccount(AccountEntity entity, AccountCsvRow import)
    {
        // Don't need to explicitly call _accountRepository.Update(entity).
        // The entity will be marked as modified if anything has changed.
        entity.Bsb = import.Bsb;
        entity.Number = import.Number;
        entity.Description = import.Description;
        entity.Balance = import.Balance;
        entity.Reserved = import.Reserved;
        entity.TotalExpenseAccrued = import.TotalExpenseAccrued;
        entity.DailyExpenseAccrual = import.DailyExpenseAccrual;
    }
}
