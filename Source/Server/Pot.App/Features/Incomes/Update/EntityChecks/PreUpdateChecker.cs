using AllOverIt.Assertion;
using AllOverIt.Patterns.ChainOfResponsibility;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Incomes.Update.EntityChecks.Checks;
using Pot.App.Features.Incomes.Update.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Incomes.Update.EntityChecks;

internal sealed class PreUpdateChecker : ChainOfResponsibilityAsyncComposer<InputState, ProblemDetailsError>, IPreUpdateChecker
{
    private readonly ILogger _logger;

    public PreUpdateChecker(IEnumerable<IPreUpdateCheck> preCheckHandlers, ILogger<PreUpdateChecker> logger)
        : base(preCheckHandlers.Cast<PreUpdateCheckBase>())
    {
        _logger = logger.WhenNotNull();
    }

    public Task<ProblemDetailsError?> CanSaveAsync(Input input, AccountEntity incomeAccount, IncomeEntity incomeToUpdate, CancellationToken cancellationToken)
    {
        var state = new InputState
        {
            Input = input,
            IncomeToUpdate = incomeToUpdate,
            IncomeAccount = incomeAccount,
        };

        return HandleAsync(state, cancellationToken);
    }
}

