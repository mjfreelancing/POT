using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Concerns.Accruals;
using Pot.App.Concerns.Accruals.Models;
using Pot.App.Concerns.Time;
using Pot.App.Errors;
using Pot.App.Extensions;
using Pot.App.Features.Expenses.Update.EntityChecks;
using Pot.App.Features.Expenses.Update.Mappings;
using Pot.App.Features.Expenses.Update.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Repositories.Expenses;
using Pot.Shared.Extensions;

namespace Pot.App.Features.Expenses.Update;

internal sealed class UpdateExpenseService : IUpdateExpenseService
{
    private readonly IAccrualDirtyStateManager _accrualDirtyStateManager;
    private readonly IPersistableExpenseRepository _expenseRepository;
    private readonly IPersistableAccountRepository _accountRepository;
    private readonly IPreUpdateChecker _preUpdateChecker;
    private readonly ITimeProvider _timeProvider;
    private readonly ILogger _logger;

    public UpdateExpenseService(IAccrualDirtyStateManager accrualDirtyStateManager, IPersistableExpenseRepository expenseRepository,
        IPersistableAccountRepository accountRepository, IPreUpdateChecker preUpdateChecker, ITimeProvider timeProvider,
        ILogger<UpdateExpenseService> logger)
    {
        _accrualDirtyStateManager = accrualDirtyStateManager.WhenNotNull();
        _expenseRepository = expenseRepository.WhenNotNull();
        _accountRepository = accountRepository.WhenNotNull();
        _preUpdateChecker = preUpdateChecker.WhenNotNull();
        _timeProvider = timeProvider.WhenNotNull();
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

            var localCurrentDate = _timeProvider.GetLocalDateNow();

            var before = GetExpenseAccrualState(expenseToUpdate);

            UpdateExpenseEntity(expenseToUpdate, input, expenseAccount, localCurrentDate);

            var after = GetExpenseAccrualState(expenseToUpdate);
            var accountIdsToMarkDirty = _accrualDirtyStateManager.GetAccountsRequiringRecalc(before, after);

            await _accrualDirtyStateManager
                .SetAccountsDirtyAsync(accountIdsToMarkDirty, cancellationToken)
                .ConfigureAwait(false);

            // Not calling _accountRepository.Update(account) as this will mark the
            // entity as modified even if nothing was changed.
            _ = await _expenseRepository.SaveAsync(cancellationToken);

            var output = expenseToUpdate.MapToOutput();

            return EnrichedResult.Success(output);
        }
    }

    private static ExpenseAccrualState GetExpenseAccrualState(ExpenseEntity expense)
    {
        return new ExpenseAccrualState
        {
            AccountId = expense.Account.Id,
            ExcludeFromCalcs = expense.ExcludeFromCalcs,
            AccrualStart = expense.AccrualStart,
            NextDue = expense.NextDue,
            EndDate = expense.EndDate,
            AccrualPolicy = expense.AccrualPolicy,
            Frequency = expense.Frequency,
            FrequencyCount = expense.FrequencyCount,
            Amount = expense.Amount
        };
    }

    private static void UpdateExpenseEntity(ExpenseEntity expenseToUpdate, Input input, AccountEntity expenseAccount, DateOnly localCurrentDate)
    {
        expenseToUpdate.ExcludeFromCalcs = input.ExcludeFromCalcs;
        expenseToUpdate.Description = input.Description;
        expenseToUpdate.AccrualStart = input.AccrualPolicy.GetCanonicalAccrualStart(input.AccrualStart, localCurrentDate);
        expenseToUpdate.NextDue = input.NextDue;
        expenseToUpdate.EndDate = input.EndDate;
        expenseToUpdate.AccrualPolicy = input.AccrualPolicy;
        expenseToUpdate.Frequency = input.Frequency;
        expenseToUpdate.FrequencyCount = input.FrequencyCount;
        expenseToUpdate.Amount = input.Amount;
        expenseToUpdate.Note = input.Note;
        expenseToUpdate.Account = expenseAccount;

        // While not all columns affect accruals, most of them do, so just mark dirty and play on the side of caution
        expenseToUpdate.AccruedIsDirty = true;
    }
}
