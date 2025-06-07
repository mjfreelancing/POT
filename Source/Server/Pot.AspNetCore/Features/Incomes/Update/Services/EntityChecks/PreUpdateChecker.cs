using AllOverIt.Assertion;
using AllOverIt.Patterns.ChainOfResponsibility;
using Pot.AspNetCore.Features.Incomes.Update.Services.EntityChecks.Checks;
using Pot.Data.Entities;
using Pot.Data.Repositories.Incomes;

namespace Pot.AspNetCore.Features.Incomes.Update.Services.EntityChecks;

internal sealed class PreUpdateChecker : ChainOfResponsibilityAsyncComposer<InputState, OutputState>, IPreUpdateChecker
{
    private static readonly IEnumerable<PreUpdateCheckBase> _handlers =
    [
        new CheckHasSameEtag(),
        new CheckDescriptionDoesNotExist()
    ];

    private readonly IIncomeRepository _incomeRepository;
    private readonly ILogger _logger;

    public PreUpdateChecker(IIncomeRepository incomeRepository, ILogger<PreUpdateChecker> logger)
        : base(_handlers)
    {
        _incomeRepository = incomeRepository.WhenNotNull();
        _logger = logger.WhenNotNull(); ;
    }

    public Task<OutputState?> CanSaveAsync(Request request, AccountEntity incomeAccount, IncomeEntity incomeToUpdate, CancellationToken cancellationToken)
    {
        var state = new InputState
        {
            Request = request,
            IncomeToUpdate = incomeToUpdate,
            IncomeAccount = incomeAccount,
            IncomeRepository = _incomeRepository,
            Logger = _logger
        };

        return HandleAsync(state, cancellationToken);
    }
}

