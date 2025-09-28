using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Calculators;
using Pot.App.Errors;
using Pot.App.Extensions;
using Pot.App.Features.Accruals.AccrueExpenses.Models;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Repositories.Expenses;

namespace Pot.App.Features.Accruals.AccrueExpenses;

internal sealed class AccrueExpensesService : IAccrueExpensesService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IPersistableExpenseRepository _expenseRepository;
    private readonly IAccrueExpenseCalculator _accrueExpenseCalculator;
    private readonly ILogger _logger;

    public AccrueExpensesService(IAccountRepository accountRepository, IPersistableExpenseRepository expenseRepository,
        IAccrueExpenseCalculator accrueExpenseCalculator, ILogger<AccrueExpensesService> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _expenseRepository = expenseRepository.WhenNotNull();
        _accrueExpenseCalculator = accrueExpenseCalculator.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<bool>> AccrueAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        using (_expenseRepository.WithTracking())
        {
            foreach (var accountRowId in input.RowIds)
            {
                var account = await _accountRepository.GetAccountAsync(accountRowId, cancellationToken).ConfigureAwait(false);

                if (account is null)
                {
                    var accountNotFoundDetails = ProblemDetailsErrorFactory.CreateEntityNotFoundError(accountRowId, "The account does not exist.");

                    _logger.LogError(accountNotFoundDetails);

                    return EnrichedResult.Fail<bool>(accountNotFoundDetails);
                }

                var expenses = await _expenseRepository.GetRenewableExpensesForAccountAsync(accountRowId, cancellationToken).ConfigureAwait(false);

                _accrueExpenseCalculator.AccrueExpenses(account, expenses);
            }

            await _expenseRepository
                .SaveAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        return EnrichedResult.Success(true);
    }
}
