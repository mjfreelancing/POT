using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.ChainOfResponsibility;
using Pot.AspNetCore.Features.Incomes.Create.Services.EntityChecks.Checks;
using Pot.Data.Entities;
using Pot.Data.Repositories.Incomes;

namespace Pot.AspNetCore.Features.Incomes.Create.Services.EntityChecks;

internal sealed class PreCreateChecker : ChainOfResponsibilityAsyncComposer<InputState, OutputState>, IPreCreateChecker
{
    private static readonly IEnumerable<PreCreateCheckBase> _handlers =
    [
        new CheckDescriptionDoesNotExist()
    ];

    private readonly IIncomeRepository _incomeRepository;
    private readonly ILogger _logger;

    public PreCreateChecker(IIncomeRepository incomeRepository, ILogger<PreCreateChecker> logger)
        : base(_handlers)
    {
        _incomeRepository = incomeRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public Task<OutputState?> CanSaveAsync(IncomeEntity incomeToCreate, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var state = new InputState
        {
            IncomeToCreate = incomeToCreate,
            IncomeRepository = _incomeRepository,
            Logger = _logger
        };

        return HandleAsync(state, cancellationToken);
    }
}

