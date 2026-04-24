using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pot.App.Concerns.Accruals;
using Pot.App.Concerns.Time;
using Pot.App.Errors;
using Pot.App.Extensions;
using Pot.Data.Repositories.AccountAccrual;
using Pot.Data.Repositories.Expenses;

namespace Pot.App.Features.Expenses.Delete;

internal sealed class DeleteExpenseService : IDeleteExpenseService
{
    private readonly IAccrualDirtyStateManager _accrualDirtyStateManager;
    private readonly IPersistableAccountAccrualRepository _accountAccrualRepository;
    private readonly IPersistableExpenseRepository _expenseRepository;
    private readonly ITimeProvider _timeProvider;
    private readonly ILogger _logger;

    public DeleteExpenseService(IAccrualDirtyStateManager accrualDirtyStateManager, IPersistableAccountAccrualRepository accountAccrualRepository,
        IPersistableExpenseRepository expenseRepository, ITimeProvider timeProvider, ILogger<DeleteExpenseService> logger)
    {
        _accrualDirtyStateManager = accrualDirtyStateManager.WhenNotNull();
        _accountAccrualRepository = accountAccrualRepository.WhenNotNull();
        _expenseRepository = expenseRepository.WhenNotNull();
        _timeProvider = timeProvider.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<bool>> DeleteExpenseAsync(Guid expenseId, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        using (_expenseRepository.WithTracking())
        {
            var expense = await _expenseRepository
                .GetExpenseOrDefaultAsync(expenseId, cancellationToken)
                .ConfigureAwait(false);

            if (expense is null)
            {
                var expenseNotFoundError = ApiDetailErrorFactory.CreateEntityNotFoundError(expenseId, "The expense does not exist");

                _logger.LogApiError(expenseNotFoundError);

                return EnrichedResult.Fail<bool>(expenseNotFoundError);
            }

            var expenseCountForAccount = await _expenseRepository.Expenses
                .CountAsync(item => item.Account.Id == expense.Account.Id, cancellationToken)
                .ConfigureAwait(false);

            var isLastExpenseForAccount = expenseCountForAccount == 1;

            if (isLastExpenseForAccount)
            {
                var accountAccrual = await _accountAccrualRepository.AccountAccruals
                    .SingleOrDefaultAsync(item => item.AccountId == expense.Account.Id, cancellationToken)
                    .ConfigureAwait(false);

                if (accountAccrual is not null)
                {
                    _accountAccrualRepository.Delete(accountAccrual);
                }
            }
            else
            {
                var localCurrentDate = _timeProvider.GetLocalDateNow();

                if (_accrualDirtyStateManager.IsExpenseDeletionImpactful(expense, localCurrentDate))
                {
                    await _accrualDirtyStateManager
                        .SetAccountsDirtyAsync([expense.Account.Id], cancellationToken)
                        .ConfigureAwait(false);
                }
            }

            _expenseRepository.Delete(expense);

            await _expenseRepository
                .SaveAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        return EnrichedResult.Success(true);
    }
}
