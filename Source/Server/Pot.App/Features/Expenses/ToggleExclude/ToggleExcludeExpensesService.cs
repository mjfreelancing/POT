using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Expenses.ToggleExclude.Models;
using Pot.Data.Repositories.Expenses;

namespace Pot.App.Features.Expenses.ToggleExclude;

/// <summary>
/// Default implementation of <see cref="IToggleExcludeExpensesService"/>.
/// </summary>
internal sealed class ToggleExcludeExpensesService : IToggleExcludeExpensesService
{
    private readonly IPersistableExpenseRepository _expenseRepository;
    private readonly ILogger _logger;

    public ToggleExcludeExpensesService(IPersistableExpenseRepository expenseRepository, ILogger<ToggleExcludeExpensesService> logger)
    {
        _expenseRepository = expenseRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    public async Task<EnrichedResult<bool>> ToggleExcludeAsync(Input input, CancellationToken cancellationToken)
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
                var expenseRenewError = ApiDetailErrorFactory.CreateBadRequestError(
                    nameof(Input.RowIds),
                    missingRowIds,
                    "One or more Expenses do not exist.");

                return EnrichedResult.Fail<bool>(expenseRenewError);
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
