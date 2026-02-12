using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.ChainOfResponsibility;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Expenses.Create.EntityChecks.Checks;
using Pot.Data.Entities;

namespace Pot.App.Features.Expenses.Create.EntityChecks;

internal sealed class PreCreateChecker : ChainOfResponsibilityAsyncComposer<InputState, ApiDetailError?>, IPreCreateChecker
{
    private readonly ILogger _logger;

    public PreCreateChecker(IEnumerable<IPreCreateCheck> preCheckHandlers, ILogger<PreCreateChecker> logger)
        : base(preCheckHandlers.Cast<PreCreateCheckBase>())
    {
        _logger = logger.WhenNotNull();
    }

    public Task<ApiDetailError?> CanSaveAsync(ExpenseEntity expenseToCreate, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var state = new InputState
        {
            ExpenseToCreate = expenseToCreate
        };

        return HandleAsync(state, cancellationToken);
    }
}

