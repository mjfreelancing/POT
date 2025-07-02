using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Calculators;
using Pot.App.Features.Accounts.AccrueExpenses.Models;
using Pot.Data.Repositories.Accounts;

namespace Pot.App.Features.Accounts.AccrueExpenses;

internal sealed class AccrueExpensesService : IAccrueExpensesService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IAccrueExpenseCalculator _accrueExpenseCalculator;
    private readonly ILogger _logger;

    public AccrueExpensesService(IAccountRepository accountRepository, IAccrueExpenseCalculator accrueExpenseCalculator,
        ILogger<AccrueExpensesService> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _accrueExpenseCalculator = accrueExpenseCalculator.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<bool>> AccrueAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        // TODO: Review all error reporting. In this case, there is no property name because it comes from a query-string parameter.
        //       Need to review for a consistent approach.



        // TODO: Update this validation to check all accounts
        //


        // TODO: Needs a transaction that covers this account repo and the expense repo used by the calculator.

        foreach (var accountRowId in input.RowIds)
        {
            // TODO: Validate the account exists - don't need it beyond this point
            var account = await _accountRepository.GetAccountAsync(accountRowId, cancellationToken).ConfigureAwait(false);

            //if (account is null)
            //{
            //    var problemDetails = ProblemDetailsErrorFactory.CreateEntityNotFoundError(
            //        "Account",
            //        string.Empty,
            //        accountRowId);

            //    return EnrichedResult.Fail<bool>(problemDetails);
            //}

            await _accrueExpenseCalculator.AccrueExpensesAsync(accountRowId, cancellationToken);
        }


        return EnrichedResult.Success(true);
    }
}
