using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Expenses.Renew.Models;
using Pot.Data.Repositories.Expenses;
using Pot.Shared.Extensions;

namespace Pot.App.Features.Expenses.Renew;

internal sealed class RenewExpensesService : IRenewExpensesService
{
    internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;

    private readonly IPersistableExpenseRepository _expenseRepository;
    private readonly ILogger _logger;

    public RenewExpensesService(IPersistableExpenseRepository expenseRepository, ILogger<RenewExpensesService> logger)
    {
        _expenseRepository = expenseRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<bool>> RenewAsync(Input input, CancellationToken cancellationToken)
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

            var today = TimeProvider.GetLocalNow().Date;
            var todayDate = DateOnly.FromDateTime(today);

            foreach (var expense in expenses)
            {
                var endDate = expense.EndDate.GetValueOrDefault(DateOnly.MaxValue);

                if (todayDate >= endDate)
                {
                    continue;
                }

                var nextDue = expense.NextDue;

                while (nextDue < todayDate)
                {
                    var days = expense.Frequency.GetDaysToNext(expense.NextDue, expense.FrequencyCount);
                    nextDue = expense.NextDue.AddDays(days);

                    // Don't advance beyond the end date
                    if (nextDue <= endDate)
                    {
                        // Not resetting / updating expense.Accrued since this impacts the account's accrued amount
                        expense.AccrualStart = expense.NextDue;
                        expense.NextDue = nextDue;
                    }
                }
            }

            await _expenseRepository.SaveAsync(cancellationToken);
        }

        return EnrichedResult.Success(true);
    }
}
