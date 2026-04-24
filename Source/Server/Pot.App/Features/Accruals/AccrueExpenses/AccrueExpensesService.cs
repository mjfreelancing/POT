using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Calculators;
using Pot.App.Concerns.Accruals;
using Pot.App.Concerns.Time;
using Pot.App.Errors;
using Pot.App.Extensions;
using Pot.App.Features.Accruals.AccrueExpenses.Models;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Repositories.Expenses;

namespace Pot.App.Features.Accruals.AccrueExpenses;

internal sealed class AccrueExpensesService : IAccrueExpensesService
{
    private readonly IAccountAccrualDirtyMarker _accountAccrualDirtyMarker;
    private readonly IAccountRepository _accountRepository;
    private readonly IPersistableExpenseRepository _expenseRepository;
    private readonly IAccrueExpenseCalculator _accrueExpenseCalculator;
    private readonly ITimeProvider _timeProvider;
    private readonly ILogger _logger;

    public AccrueExpensesService(IAccountAccrualDirtyMarker accountAccrualDirtyMarker, IAccountRepository accountRepository,
        IPersistableExpenseRepository expenseRepository, IAccrueExpenseCalculator accrueExpenseCalculator,
        ITimeProvider timeProvider, ILogger<AccrueExpensesService> logger)
    {
        _accountAccrualDirtyMarker = accountAccrualDirtyMarker.WhenNotNull();
        _accountRepository = accountRepository.WhenNotNull();
        _expenseRepository = expenseRepository.WhenNotNull();
        _accrueExpenseCalculator = accrueExpenseCalculator.WhenNotNull();
        _timeProvider = timeProvider.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<bool>> AccrueAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        using (_expenseRepository.WithTracking())
        {
            foreach (var accountRowId in input.RowIds)
            {
                var localCurrentDate = _timeProvider.GetLocalDateNow();

                var account = await _accountRepository
                    .GetAccountOrDefaultAsync(accountRowId, cancellationToken)
                    .ConfigureAwait(false);

                if (account is null)
                {
                    var accountNotFoundError = ApiDetailErrorFactory.CreateEntityNotFoundError(accountRowId, "The account does not exist");

                    _logger.LogApiError(accountNotFoundError);

                    return EnrichedResult.Fail<bool>(accountNotFoundError);
                }

                var expenses = await _expenseRepository.GetExpensesForAccountAsync(accountRowId, cancellationToken).ConfigureAwait(false);

                _accrueExpenseCalculator.AccrueExpenses(account, expenses, localCurrentDate);

                await _accountAccrualDirtyMarker
                    .ClearDirtyOnAccrualSuccessAsync(account, localCurrentDate, cancellationToken)
                    .ConfigureAwait(false);
            }

            await _expenseRepository
                .SaveAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        return EnrichedResult.Success(true);
    }
}
