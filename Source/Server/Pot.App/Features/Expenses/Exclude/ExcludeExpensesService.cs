using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Expenses.Exclude.Models;
using Pot.Data.Repositories.Expenses;

namespace Pot.App.Features.Expenses.Exclude;

internal sealed class ExcludeExpensesService : IExcludeExpensesService
{
    private readonly IPersistableExpenseRepository _expenseRepository;
    private readonly ILogger _logger;

    public ExcludeExpensesService(IPersistableExpenseRepository expenseRepository, ILogger<ExcludeExpensesService> logger)
    {
        _expenseRepository = expenseRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<bool>> ToggleExclusionAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        using (_expenseRepository.WithTracking())
        {
            var expenses = await _expenseRepository.GetExpensesAsync(input.RowIds, cancellationToken);

            var missingRowIds = input.RowIds
                .Except(expenses.Select(e => e.RowId))
                .ToArray();

            if (missingRowIds.Length > 0)
            {
                var expenseRenewProblem = ProblemDetailsErrorFactory.CreateUnprocessableEntityError(
                    nameof(Input.RowIds),
                    missingRowIds,
                    "One or more Expenses do not exist.");

                return EnrichedResult.Fail<bool>(expenseRenewProblem);
            }

            foreach (var expense in expenses)
            {
                expense.ExcludeFromCalcs = !expense.ExcludeFromCalcs;
            }

            await _expenseRepository.SaveAsync(cancellationToken);
        }

        return EnrichedResult.Success(true);
    }
}
