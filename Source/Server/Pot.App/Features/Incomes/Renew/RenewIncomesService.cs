using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Calculators;
using Pot.App.Errors;
using Pot.App.Features.Incomes.Renew.Models;
using Pot.Data.Repositories.Incomes;

namespace Pot.App.Features.Incomes.Renew;

internal sealed class RenewExpensesService : IRenewIncomesService
{
    private readonly IPersistableIncomeRepository _incomeRepository;
    private readonly IIncomeRenewalCalculator _renewalCalculator;
    private readonly ILogger _logger;

    public RenewExpensesService(IPersistableIncomeRepository incomeRepository, IIncomeRenewalCalculator renewalCalculator,
        ILogger<RenewExpensesService> logger)
    {
        _incomeRepository = incomeRepository.WhenNotNull();
        _renewalCalculator = renewalCalculator.WhenNotNull();
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
                var incomeRenewError = ApiDetailErrorFactory.CreateUnprocessableEntityError(
                    nameof(Input.RowIds),
                    missingRowIds,
                    "One or more Incomes do not exist.");

                return EnrichedResult.Fail<bool>(incomeRenewError);
            }

            _renewalCalculator.Renew(incomes, input.Mode, input.AsOfDate);

            await _incomeRepository.SaveAsync(cancellationToken);
        }

        return EnrichedResult.Success(true);
    }
}
