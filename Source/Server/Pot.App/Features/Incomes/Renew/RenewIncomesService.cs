using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Incomes.Renew.Models;
using Pot.Data.Repositories.Incomes;
using Pot.Shared.Extensions;

namespace Pot.App.Features.Incomes.Renew;

internal sealed class RenewExpensesService : IRenewIncomesService
{
    internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;

    private readonly IPersistableIncomeRepository _incomeRepository;
    private readonly ILogger _logger;

    public RenewExpensesService(IPersistableIncomeRepository incomeRepository, ILogger<RenewExpensesService> logger)
    {
        _incomeRepository = incomeRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<bool>> RenewAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        using (_incomeRepository.WithTracking())
        {
            var incomes = await _incomeRepository.GetIncomesAsync(input.RowIds, cancellationToken);

            var missingRowIds = input.RowIds
                .Except(incomes.Select(e => e.RowId))
                .ToArray();

            if (missingRowIds.Length > 0)
            {
                var incomeRenewProblem = ProblemDetailsErrorFactory.CreateUnprocessableEntityError(
                    nameof(Input.RowIds),
                    missingRowIds,
                    "One or more Incomes do not exist.");

                return EnrichedResult.Fail<bool>(incomeRenewProblem);
            }

            var today = TimeProvider.GetLocalNow().Date;
            var todayDate = DateOnly.FromDateTime(today);

            foreach (var income in incomes)
            {
                var endDate = income.EndDate.GetValueOrDefault(DateOnly.MaxValue);

                if (todayDate >= endDate)
                {
                    continue;
                }

                var nextDue = income.NextDue;

                while (nextDue < todayDate)
                {
                    var days = income.Frequency.GetDaysToNext(income.NextDue, income.FrequencyCount);
                    nextDue = income.NextDue.AddDays(days);

                    // Don't advance beyond the end date
                    if (nextDue <= endDate)
                    {
                        income.NextDue = nextDue;
                    }
                }
            }

            await _incomeRepository.SaveAsync(cancellationToken);
        }

        return EnrichedResult.Success(true);
    }
}
