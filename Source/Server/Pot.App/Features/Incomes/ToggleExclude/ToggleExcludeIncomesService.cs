using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Incomes.ToggleExclude.Models;
using Pot.Data.Repositories.Incomes;

namespace Pot.App.Features.Incomes.ToggleExclude;

/// <summary>
/// Default implementation of <see cref="IToggleExcludeIncomesService"/>.
/// </summary>
internal sealed class ToggleExcludeIncomesService : IToggleExcludeIncomesService
{
    private readonly IPersistableIncomeRepository _incomeRepository;
    private readonly ILogger _logger;

    public ToggleExcludeIncomesService(IPersistableIncomeRepository incomeRepository, ILogger<ToggleExcludeIncomesService> logger)
    {
        _incomeRepository = incomeRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    public async Task<EnrichedResult<bool>> ToggleExcludeAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        using (_incomeRepository.WithTracking())
        {
            var incomes = await _incomeRepository.GetIncomesAsync(input.RowIds, cancellationToken);

            var missingRowIds = input.RowIds
                .Except(incomes.Select(income => income.RowId))
                .ToArray();

            if (missingRowIds.Length > 0)
            {
                var expenseRenewError = ApiDetailErrorFactory.CreateUnprocessableEntityError(
                    nameof(Input.RowIds),
                    missingRowIds,
                    "One or more Incomes do not exist.");

                return EnrichedResult.Fail<bool>(expenseRenewError);
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
