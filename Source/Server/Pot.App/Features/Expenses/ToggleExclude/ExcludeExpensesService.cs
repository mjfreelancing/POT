using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Concerns.Accruals;
using Pot.App.Errors;
using Pot.App.Features.Expenses.ToggleExclude.Models;
using Pot.Data.Repositories.Expenses;

namespace Pot.App.Features.Expenses.ToggleExclude;

internal sealed class ExcludeExpensesService : IExcludeExpensesService
{
    private readonly IAccountAccrualDirtyMarker _accountAccrualDirtyMarker;
    private readonly IPersistableExpenseRepository _expenseRepository;
    private readonly ILogger _logger;

    public ExcludeExpensesService(IAccountAccrualDirtyMarker accountAccrualDirtyMarker, IPersistableExpenseRepository expenseRepository, ILogger<ExcludeExpensesService> logger)
    {
        _accountAccrualDirtyMarker = accountAccrualDirtyMarker.WhenNotNull();
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

            foreach (var expense in expenses)
            {
                expense.ExcludeFromCalcs = !expense.ExcludeFromCalcs;
                expense.AccruedIsDirty = true;
            }

            await _accountAccrualDirtyMarker
                .MarkDirtyForExpensesAsync(expenses, cancellationToken)
                .ConfigureAwait(false);

            await _expenseRepository.SaveAsync(cancellationToken);
        }

        return EnrichedResult.Success(true);
    }
}
