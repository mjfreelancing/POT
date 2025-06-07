using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.ChainOfResponsibility;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Accounts.Delete.EntityChecks.Checks;

namespace Pot.App.Features.Accounts.Delete.EntityChecks;

internal sealed class PreDeleteChecker : ChainOfResponsibilityAsyncComposer<InputState, ProblemDetailsError?>, IPreCreateChecker
{
    private readonly ILogger _logger;

    public PreDeleteChecker(IEnumerable<IPreDeleteCheck> preCheckHandlers, ILogger<PreDeleteChecker> logger)
        : base(preCheckHandlers.Cast<PreDeleteCheckBase>())
    {
        _logger = logger.WhenNotNull();
    }

    public Task<ProblemDetailsError?> CanSaveAsync(Guid accountId, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var state = new InputState
        {
            AccountId = accountId
        };

        return HandleAsync(state, cancellationToken);
    }
}

