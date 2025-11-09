using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.ChainOfResponsibility;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Approvals.UpdateStatus.EntityChecks.Checks;
using Pot.App.Features.Approvals.UpdateStatus.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Approvals.UpdateStatus.EntityChecks;

internal sealed class PreUpdateChecker : ChainOfResponsibilityAsyncComposer<InputState, ProblemDetailsError>, IPreUpdateChecker
{
    private readonly ILogger _logger;

    public PreUpdateChecker(IEnumerable<IPreUpdateCheck> preCheckHandlers, ILogger<PreUpdateChecker> logger)
        : base(preCheckHandlers.Cast<PreUpdateCheckBase>())
    {
        _logger = logger.WhenNotNull();
    }

    public Task<ProblemDetailsError?> CanSaveAsync(Input input, UserEntity user, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var state = new InputState
        {
            Input = input,
            UserToUpdate = user
        };

        return HandleAsync(state, cancellationToken);
    }
}

