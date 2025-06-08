using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.Data.Repositories.Incomes;

namespace Pot.App.Features.Incomes.Delete;

internal sealed class DeleteIncomeService : IDeleteIncomeService
{
    private readonly IPersistableIncomeRepository _incomeRepository;
    private readonly ILogger _logger;

    public DeleteIncomeService(IPersistableIncomeRepository incomeRepository, ILogger<DeleteIncomeService> logger)
    {
        _incomeRepository = incomeRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<bool>> DeleteIncomeAsync(Guid incomeId, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var income = await _incomeRepository.GetIncomeOrDefaultAsync(incomeId, cancellationToken);

        if (income is null)
        {
            return EnrichedResult.Success(false);
        }

        _incomeRepository.Delete(income);

        await _incomeRepository.SaveAsync(cancellationToken);

        return EnrichedResult.Success(true);
    }
}
