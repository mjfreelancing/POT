using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Features.Incomes.Create;
using Pot.App.Features.Incomes.Update;
using Pot.App.Features.Maintenance.Import.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Repositories.Incomes;

namespace Pot.App.Features.Maintenance.Import.Incomes;

internal sealed class IncomesImporter : IIncomesImporter
{
    private readonly IPersistableAccountRepository _accountRepository;
    private readonly IIncomeRepository _incomeRepository;
    private readonly ICreateIncomeService _createIncomeService;
    private readonly IUpdateIncomeService _updateIncomeService;
    private readonly ILogger<IncomesImporter> _logger;

    public IncomesImporter(IPersistableAccountRepository accountRepository, IIncomeRepository incomeRepository,
        ICreateIncomeService createIncomeService, IUpdateIncomeService updateIncomeService,
        ILogger<IncomesImporter> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _incomeRepository = incomeRepository.WhenNotNull();
        _createIncomeService = createIncomeService.WhenNotNull();
        _updateIncomeService = updateIncomeService.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<int>> ImportAsync(IEnumerable<IIncomeCsvRow> csvRows, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var count = 0;

        using (_accountRepository.WithTracking())
        {
            var accounts = new Dictionary<Guid, AccountEntity>();

            foreach (var csvRow in csvRows)
            {
                if (!accounts.TryGetValue(csvRow.AccountRowId, out var account))
                {
                    // Not validating the rows since the CSV file is expected to be valid (the data was previously exported).
                    account = await _accountRepository
                        .GetAccountAsync(csvRow.AccountRowId, cancellationToken)
                        .ConfigureAwait(false);

                    accounts.Add(csvRow.AccountRowId, account);
                }

                await CreateOrUpdateIncomeAsync(account, csvRow, cancellationToken).ConfigureAwait(false);

                count++;
            }

            await _accountRepository
                .SaveAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        return EnrichedResult.Success(count);
    }

    private async Task<EnrichedResult<int>> CreateOrUpdateIncomeAsync(AccountEntity account, IIncomeCsvRow csvRow, CancellationToken cancellationToken)
    {
        var csvIncomeId = csvRow.RowId;

        var incomeEntity = await _incomeRepository
            .GetIncomeOrDefaultAsync(csvIncomeId, cancellationToken)
            .ConfigureAwait(false);

        return incomeEntity is null
            ? await CreateIncomeAsync(account.RowId, csvRow, cancellationToken).ConfigureAwait(false)
            : await UpdateIncomeAsync(account.RowId, incomeEntity.Etag, csvRow, cancellationToken).ConfigureAwait(false);
    }

    private async Task<EnrichedResult<int>> CreateIncomeAsync(Guid accountRowId, IIncomeCsvRow import,
        CancellationToken cancellationToken)
    {
        var input = new Features.Incomes.Create.Models.Input
        {
            Description = import.Description,
            NextDue = import.NextDue,
            EndDate = import.EndDate,
            Frequency = import.Frequency,
            FrequencyCount = import.FrequencyCount,
            Amount = import.Amount,
            Note = import.Note,
            AccountRowId = accountRowId
        };

        var createResult = await _createIncomeService
            .CreateIncomeAsync(input, cancellationToken)
            .ConfigureAwait(false);

        return createResult.IsSuccess
            ? EnrichedResult.Success<int>()
            : EnrichedResult.Fail<int>(createResult.Error);
    }

    private async Task<EnrichedResult<int>> UpdateIncomeAsync(Guid accountRowId, long incomeEtag, IIncomeCsvRow csvRow,
        CancellationToken cancellationToken)
    {
        var input = new Features.Incomes.Update.Models.Input
        {
            RowId = csvRow.RowId,
            Etag = incomeEtag,
            ExcludeFromCalcs = csvRow.ExcludeFromCalcs,
            Description = csvRow.Description,
            NextDue = csvRow.NextDue,
            EndDate = csvRow.EndDate,
            Frequency = csvRow.Frequency,
            FrequencyCount = csvRow.FrequencyCount,
            Amount = csvRow.Amount,
            Note = csvRow.Note,
            AccountRowId = accountRowId
        };

        var updateResult = await _updateIncomeService
            .UpdateIncomeAsync(input, cancellationToken)
            .ConfigureAwait(false);

        return updateResult.IsSuccess
            ? EnrichedResult.Success<int>()
            : EnrichedResult.Fail<int>(updateResult.Error);
    }
}
