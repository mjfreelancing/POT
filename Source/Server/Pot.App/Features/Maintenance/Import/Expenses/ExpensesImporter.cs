using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Features.Expenses.Create;
using Pot.App.Features.Expenses.Update;
using Pot.App.Features.Maintenance.Import.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Repositories.Expenses;

namespace Pot.App.Features.Maintenance.Import.Expenses;

internal sealed class ExpensesImporter : IExpensesImporter
{
    private readonly IPersistableAccountRepository _accountRepository;
    private readonly IExpenseRepository _expenseRepository;
    private readonly ICreateExpenseService _createExpenseService;
    private readonly IUpdateExpenseService _updateExpenseService;
    private readonly ILogger<ExpensesImporter> _logger;

    public ExpensesImporter(IPersistableAccountRepository accountRepository, IExpenseRepository expenseRepository,
        ICreateExpenseService createExpenseService, IUpdateExpenseService updateExpenseService,
        ILogger<ExpensesImporter> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _expenseRepository = expenseRepository.WhenNotNull();
        _createExpenseService = createExpenseService.WhenNotNull();
        _updateExpenseService = updateExpenseService.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<int>> ImportAsync(IEnumerable<IExpenseCsvRow> csvRows, CancellationToken cancellationToken)
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

                await CreateOrUpdateExpenseAsync(account, csvRow, cancellationToken).ConfigureAwait(false);

                count++;
            }

            await _accountRepository
                .SaveAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        return EnrichedResult.Success(count);
    }

    private async Task<EnrichedResult<int>> CreateOrUpdateExpenseAsync(AccountEntity account, IExpenseCsvRow csvRow, CancellationToken cancellationToken)
    {
        var csvExpenseId = csvRow.RowId;

        var expenseEntity = await _expenseRepository
            .GetExpenseOrDefaultAsync(csvExpenseId, cancellationToken)
            .ConfigureAwait(false);

        return expenseEntity is null
            ? await CreateExpenseAsync(account.RowId, csvRow, cancellationToken).ConfigureAwait(false)
            : await UpdateExpenseAsync(account.RowId, expenseEntity.Etag, csvRow, cancellationToken).ConfigureAwait(false);
    }

    private async Task<EnrichedResult<int>> CreateExpenseAsync(Guid accountRowId, IExpenseCsvRow import,
        CancellationToken cancellationToken)
    {
        var input = new Features.Expenses.Create.Models.Input
        {
            ExcludeFromCalcs = import.ExcludeFromCalcs,
            Description = import.Description,
            AccrualStart = import.AccrualStart,
            NextDue = import.NextDue,
            EndDate = import.EndDate,
            Frequency = import.Frequency,
            FrequencyCount = import.FrequencyCount,
            Amount = import.Amount,
            Note = import.Note,
            AccountRowId = accountRowId

            // Not imported - all calculations would need to be refreshed anyway
            // Accrued = import.Accrued,
            // AccruedIsDirty = import.AccruedIsDirty,
            // LastAccruedUpdate = import.LastAccruedUpdate,
        };

        var createResult = await _createExpenseService
            .CreateExpenseAsync(input, cancellationToken)
            .ConfigureAwait(false);

        return createResult.IsSuccess
            ? EnrichedResult.Success<int>()
            : EnrichedResult.Fail<int>(createResult.Error);
    }

    private async Task<EnrichedResult<int>> UpdateExpenseAsync(Guid accountRowId, long expenseEtag, IExpenseCsvRow csvRow,
        CancellationToken cancellationToken)
    {
        var input = new Features.Expenses.Update.Models.Input
        {
            RowId = csvRow.RowId,
            Etag = expenseEtag,
            ExcludeFromCalcs = csvRow.ExcludeFromCalcs,
            Description = csvRow.Description,
            AccrualStart = csvRow.AccrualStart,
            NextDue = csvRow.NextDue,
            EndDate = csvRow.EndDate,
            Frequency = csvRow.Frequency,
            FrequencyCount = csvRow.FrequencyCount,
            Amount = csvRow.Amount,
            Note = csvRow.Note,
            AccountRowId = accountRowId

            // Not imported - all calculations would need to be refreshed anyway
            // Accrued = import.Accrued,
            // AccruedIsDirty = import.AccruedIsDirty,
            // LastAccruedUpdate = import.LastAccruedUpdate,
        };

        var updateResult = await _updateExpenseService
            .UpdateExpenseAsync(input, cancellationToken)
            .ConfigureAwait(false);

        return updateResult.IsSuccess
            ? EnrichedResult.Success<int>()
            : EnrichedResult.Fail<int>(updateResult.Error);
    }
}
