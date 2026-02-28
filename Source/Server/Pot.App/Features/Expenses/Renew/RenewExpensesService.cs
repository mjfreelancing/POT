using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Calculators;
using Pot.App.Errors;
using Pot.App.Features.Expenses.Renew.Models;
using Pot.Data.Repositories.Expenses;

namespace Pot.App.Features.Expenses.Renew;

internal sealed class RenewExpensesService : IRenewExpensesService
{
    private readonly IPersistableExpenseRepository _expenseRepository;
    private readonly IExpenseRenewalCalculator _renewalCalculator;
    private readonly ILogger _logger;

    public RenewExpensesService(IPersistableExpenseRepository expenseRepository, IExpenseRenewalCalculator renewalCalculator,
        ILogger<RenewExpensesService> logger)
    {
        _expenseRepository = expenseRepository.WhenNotNull();
        _renewalCalculator = renewalCalculator.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<bool>> RenewAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        using (_expenseRepository.WithTracking())
        {
            var expenses = await _expenseRepository.GetExpensesAsync(input.RowIds, cancellationToken);

            var missingRowIds = input.RowIds
                .Except(expenses.Select(expense => expense.RowId))
                .ToArray();

            if (missingRowIds.Length > 0)
            {
                var expenseRenewError = ApiDetailErrorFactory.CreateUnprocessableEntityError(
                    nameof(Input.RowIds),
                    missingRowIds,
                    "One or more Expenses do not exist.");

                return EnrichedResult.Fail<bool>(expenseRenewError);
            }

            _renewalCalculator.Renew(expenses, input.Mode, input.AsOfDate);

            await _expenseRepository.SaveAsync(cancellationToken);
        }

        return EnrichedResult.Success(true);
    }
}
