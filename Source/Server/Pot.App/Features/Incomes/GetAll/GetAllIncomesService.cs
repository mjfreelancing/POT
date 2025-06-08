using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Pagination;
using Microsoft.Extensions.Logging;
using Pot.App.Features.Incomes.GetAll.Mappings;
using Pot.App.Features.Incomes.GetAll.Models;
using Pot.Data.Repositories.Incomes;
using Pot.Shared;

namespace Pot.App.Features.Incomes.GetAll;

internal sealed class GetAllIncomesService : IGetAllIncomesService
{
    private readonly IIncomeRepository _incomeRepository;
    private readonly ILogger _logger;

    public GetAllIncomesService(IIncomeRepository incomeRepository, ILogger<GetAllIncomesService> logger)
    {
        _incomeRepository = incomeRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<PageResult<Output>> GetAllIncomesAsync(Paging paging, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var incomes = await _incomeRepository.GetAllIncomesAsync(paging, cancellationToken);

        var outputs = incomes.Results.Select(result => result.MapToOutput());

        return PageResult<Output>.CreateFrom(incomes, outputs);
    }
}
