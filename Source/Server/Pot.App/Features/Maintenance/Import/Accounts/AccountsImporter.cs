using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Features.Accounts.Create;
using Pot.App.Features.Accounts.Update;
using Pot.App.Features.Maintenance.Import.Models;
using Pot.Data.Repositories.Accounts;

namespace Pot.App.Features.Maintenance.Import.Accounts;

internal sealed class AccountsImporter : IAccountsImporter
{
    private readonly IPersistableAccountRepository _accountRepository;
    private readonly ICreateAccountService _createAccountService;
    private readonly IUpdateAccountService _updateAccountService;
    private readonly ILogger<AccountsImporter> _logger;

    public AccountsImporter(IPersistableAccountRepository accountRepository, ICreateAccountService createAccountService,
        IUpdateAccountService updateAccountService, ILogger<AccountsImporter> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _createAccountService = createAccountService.WhenNotNull();
        _updateAccountService = updateAccountService.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<int>> ImportAsync(IEnumerable<IAccountCsvRow> csvRows, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var count = 0;

        using (_accountRepository.WithTracking())
        {
            // Not validating the rows since the CSV file is expected to be valid (the data was previously exported).
            foreach (var csvRow in csvRows)
            {
                var result = await CreateOrUpdateAccountAsync(csvRow, cancellationToken).ConfigureAwait(false);

                if (!result.IsSuccess)
                {
                    return result;
                }

                count++;
            }

            // Could throw UniqueConstraintException (or a related database exception),
            // resulting in a custom 422 ProblemDetails response.
            await _accountRepository
                .SaveAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        return EnrichedResult.Success(count);
    }

    private async Task<EnrichedResult<int>> CreateOrUpdateAccountAsync(IAccountCsvRow csvRow, CancellationToken cancellationToken)
    {
        var csvAccountId = csvRow.RowId;

        var accountEntity = await _accountRepository
            .GetAccountOrDefaultAsync(csvAccountId, cancellationToken)
            .ConfigureAwait(false);

        return accountEntity is null
            ? await CreateAccountAsync(csvRow, cancellationToken).ConfigureAwait(false)
            : await UpdateAccountAsync(accountEntity.Etag, csvRow, cancellationToken).ConfigureAwait(false);
    }

    private async Task<EnrichedResult<int>> CreateAccountAsync(IAccountCsvRow csvRow, CancellationToken cancellationToken)
    {
        var input = new Features.Accounts.Create.Models.Input
        {
            RowId = csvRow.RowId,
            Bsb = csvRow.Bsb,
            Number = csvRow.Number,
            Description = csvRow.Description,
            Balance = csvRow.Balance,
            Reserved = csvRow.Reserved

            // Not imported - all calculations would need to be refreshed anyway
            // TotalExpenseAccrued = csvRow.TotalExpenseAccrued,
            // DailyExpenseAccrual = csvRow.DailyExpenseAccrual,
            // StableExpenseAccrual = csvRow.StableExpenseAccrual
        };

        var createResult = await _createAccountService
            .CreateAccountAsync(input, cancellationToken)
            .ConfigureAwait(false);

        return createResult.IsSuccess
            ? EnrichedResult.Success<int>()
            : EnrichedResult.Fail<int>(createResult.Error);
    }

    private async Task<EnrichedResult<int>> UpdateAccountAsync(long accountEtag, IAccountCsvRow csvRow, CancellationToken cancellationToken)
    {
        var input = new Features.Accounts.Update.Models.Input
        {
            Etag = accountEtag,
            RowId = csvRow.RowId,
            Bsb = csvRow.Bsb,
            Number = csvRow.Number,
            Description = csvRow.Description,
            Balance = csvRow.Balance,
            Reserved = csvRow.Reserved,

            // Not imported - all calculations would need to be refreshed anyway
            // TotalExpenseAccrued = csvRow.TotalExpenseAccrued,
            // DailyExpenseAccrual = csvRow.DailyExpenseAccrual,
            // StableExpenseAccrual = csvRow.StableExpenseAccrual
        };

        var updateResult = await _updateAccountService
            .UpdateAccountAsync(input, cancellationToken)
            .ConfigureAwait(false);

        return updateResult.IsSuccess
            ? EnrichedResult.Success<int>()
            : EnrichedResult.Fail<int>(updateResult.Error);
    }
}