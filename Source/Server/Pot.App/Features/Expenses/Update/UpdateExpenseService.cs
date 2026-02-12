using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Extensions;
using Pot.App.Features.Expenses.Update.EntityChecks;
using Pot.App.Features.Expenses.Update.Mappings;
using Pot.App.Features.Expenses.Update.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Repositories.Expenses;

namespace Pot.App.Features.Expenses.Update;

internal sealed class UpdateExpenseService : IUpdateExpenseService
{
    private readonly IPersistableExpenseRepository _expenseRepository;
    private readonly IPersistableAccountRepository _accountRepository;
    private readonly IPreUpdateChecker _preUpdateChecker;
    private readonly ILogger _logger;

    public UpdateExpenseService(IPersistableExpenseRepository expenseRepository, IPersistableAccountRepository accountRepository,
        IPreUpdateChecker preUpdateChecker, ILogger<UpdateExpenseService> logger)
    {
        _expenseRepository = expenseRepository.WhenNotNull();
        _accountRepository = accountRepository.WhenNotNull(); ;
        _preUpdateChecker = preUpdateChecker.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<Output>> UpdateExpenseAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        using (_expenseRepository.WithTracking())
        {
            var expenseId = input.RowId;

            var expenseToUpdate = await _expenseRepository
                .GetExpenseOrDefaultAsync(expenseId, cancellationToken)
                .ConfigureAwait(false);

            if (expenseToUpdate is null)
            {
                var expenseNotFoundError = ApiDetailErrorFactory.CreateEntityNotFoundError(expenseId, "The expense does not exist");

                _logger.LogApiError(expenseNotFoundError);

                return EnrichedResult.Fail<Output>(expenseNotFoundError);
            }

            var expenseAccount = await _accountRepository
                .GetAccountOrDefaultAsync(input.AccountRowId, cancellationToken)
                .ConfigureAwait(false);

            if (expenseAccount is null)
            {
                var expenseAccountNotFoundError = ApiDetailErrorFactory.CreateEntityNotFoundError(input.AccountRowId, "The account does not exist");

                _logger.LogApiError(expenseAccountNotFoundError);

                return EnrichedResult.Fail<Output>(expenseAccountNotFoundError);
            }

            var apiError = await _preUpdateChecker
                .CanSaveAsync(input, expenseAccount, expenseToUpdate, cancellationToken)
                .ConfigureAwait(false);

            if (apiError is not null)
            {
                _logger.LogApiError(apiError);

                return EnrichedResult.Fail<Output>(apiError);
            }

            UpdateExpenseEntity(expenseToUpdate, input, expenseAccount);

            // Not calling _accountRepository.Update(account) as this will mark the
            // entity as modified even if nothing was changed.
            _ = await _expenseRepository.SaveAsync(cancellationToken);

            var output = expenseToUpdate.MapToOutput();

            return EnrichedResult.Success(output);
        }
    }

    private static void UpdateExpenseEntity(ExpenseEntity expenseToUpdate, Input input, AccountEntity expenseAccount)
    {
        expenseToUpdate.ExcludeFromCalcs = input.ExcludeFromCalcs;
        expenseToUpdate.Description = input.Description;
        expenseToUpdate.AccrualStart = input.AccrualStart;
        expenseToUpdate.NextDue = input.NextDue;
        expenseToUpdate.EndDate = input.EndDate;
        expenseToUpdate.Frequency = input.Frequency;
        expenseToUpdate.FrequencyCount = input.FrequencyCount;
        expenseToUpdate.Amount = input.Amount;
        expenseToUpdate.Note = input.Note;
        expenseToUpdate.Account = expenseAccount;

        // While not all columns affect accruals, most of them do, so just mark dirty and play on the side of caution
        expenseToUpdate.AccruedIsDirty = true;
    }
}
