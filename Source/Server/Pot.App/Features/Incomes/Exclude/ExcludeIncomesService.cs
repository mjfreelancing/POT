using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Incomes.Exclude.Models;
using Pot.Data.Repositories.Incomes;

namespace Pot.App.Features.Incomes.Exclude;

internal sealed class ExcludeIncomesService : IExcludeIncomesService
{
    private readonly IPersistableIncomeRepository _incomeRepository;
    private readonly ILogger _logger;

    public ExcludeIncomesService(IPersistableIncomeRepository incomeRepository, ILogger<ExcludeIncomesService> logger)
    {
        _incomeRepository = incomeRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<bool>> ToggleExclusionAsync(Input input, CancellationToken cancellationToken)
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
                var expenseRenewProblem = ProblemDetailsErrorFactory.CreateUnprocessableEntityError(
                    nameof(Input.RowIds),
                    missingRowIds,
                    "One or more Incomes do not exist.");

                return EnrichedResult.Fail<bool>(expenseRenewProblem);
            }

            foreach (var income in incomes)
            {
                income.ExcludeFromCalcs = !income.ExcludeFromCalcs;
            }

            await _incomeRepository.SaveAsync(cancellationToken);
        }

        return EnrichedResult.Success(true);
    }
}
