using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Calculators;
using Pot.App.Errors;
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

    public async Task<EnrichedResult<bool>> AccrueAsync(Guid accountRowId, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var account = await _accountRepository.GetAccountOrDefaultAsync(accountRowId, cancellationToken).ConfigureAwait(false);

        // TODO: Review all error reporting. In this case, there is no property name because it comes from a query-string parameter.
        //       Need to review for a consistent approach.
        if (account is null)
        {
            var problemDetails = ProblemDetailsErrorFactory.CreateEntityNotFoundError(
                "Account",
                string.Empty,
                accountRowId);

            return EnrichedResult.Fail<bool>(problemDetails);
        }

        await _accrueExpenseCalculator.AccrueExpensesAsync(account, cancellationToken);

        return EnrichedResult.Success(true);
    }
}
