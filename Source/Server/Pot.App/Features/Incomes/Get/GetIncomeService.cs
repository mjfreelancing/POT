using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Features.Incomes.Get.Mappings;
using Pot.App.Features.Incomes.Get.Models;
using Pot.Data.Repositories.Incomes;

namespace Pot.App.Features.Incomes.Get;

internal sealed class GetIncomeService : IGetIncomeService
{
    private readonly IPersistableIncomeRepository _incomeRepository;
    private readonly ILogger _logger;

    public GetIncomeService(IPersistableIncomeRepository incomeRepository, ILogger<GetIncomeService> logger)
    {
        _incomeRepository = incomeRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<Output?> GetIncomeAsync(Guid incomeId, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var income = await _incomeRepository.GetIncomeOrDefaultAsync(incomeId, cancellationToken);

        return income?.MapToOutput();
    }
}
