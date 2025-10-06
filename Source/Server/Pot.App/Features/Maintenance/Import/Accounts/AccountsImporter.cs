using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Features.Maintenance.Import.Models;
using Pot.Data;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;

namespace Pot.App.Features.Maintenance.Import.Accounts;

internal sealed class AccountsImporter : IAccountsImporter
{
    private readonly IPersistableAccountRepository _accountRepository;
    private readonly ICurrentUserDataContext _currentUserDataContext;
    private readonly ILogger<AccountsImporter> _logger;

    public AccountsImporter(IPersistableAccountRepository accountRepository, ICurrentUserDataContext currentUserContext,
        ILogger<AccountsImporter> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _currentUserDataContext = currentUserContext.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<int> ImportAsync(IEnumerable<IAccountCsvRow> csvRows, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var count = 0;

        using (_accountRepository.WithTracking())
        {
            var user = await _currentUserDataContext.GetUserAsync();

            // Not validating the rows since the CSV file is expected to be valid (the data was previously exported).
            foreach (var csvRow in csvRows)
            {
                await CreateOrUpdateAccountAsync(user.Site, csvRow, cancellationToken).ConfigureAwait(false);

                count++;
            }

            // Could throw UniqueConstraintException (or a related database exception),
            // resulting in a custom 422 ProblemDetails response.
            await _accountRepository
                .SaveAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        return count;
    }

    private async Task CreateOrUpdateAccountAsync(SiteEntity site, IAccountCsvRow csvRow, CancellationToken cancellationToken)
    {
        var csvAccountId = csvRow.RowId;

        var accountEntity = await _accountRepository
            .GetAccountOrDefaultAsync(csvAccountId, cancellationToken)
            .ConfigureAwait(false);

        if (accountEntity is null)
        {
            CreateAccountEntity(site, csvRow);
        }
        else
        {
            UpdateExistingAccount(accountEntity, csvRow);
        }
    }

    private AccountEntity CreateAccountEntity(SiteEntity site, IAccountCsvRow import)
    {
        var accountEntity = new AccountEntity
        {
            Site = site,
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

    private static void UpdateExistingAccount(AccountEntity entity, IAccountCsvRow import)
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
