using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Features.Incomes.GetAll.Mappings;
using Pot.App.Features.Incomes.GetAll.Models;
using Pot.Data.Repositories.Incomes;

namespace Pot.App.Features.Incomes.GetAll;

internal sealed class GetIncomesService : IGetIncomesService
{
    private readonly IIncomeRepository _incomeRepository;
    private readonly ILogger _logger;

    public GetIncomesService(IIncomeRepository incomeRepository, ILogger<GetIncomesService> logger)
    {
        _incomeRepository = incomeRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<List<Output>> GetAllIncomesAsync(CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var incomes = await _incomeRepository
            .GetAllIncomesAsync(cancellationToken)
            .ConfigureAwait(false);

        return incomes.SelectToList(income => income.MapToOutput());
    }
}
