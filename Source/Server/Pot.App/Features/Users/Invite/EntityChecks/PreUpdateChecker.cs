using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.ChainOfResponsibility;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Users.Invite.EntityChecks.Checks;
using Pot.App.Features.Users.Invite.Models;

namespace Pot.App.Features.Users.Invite.EntityChecks;

internal sealed class PreUpdateChecker : ChainOfResponsibilityAsyncComposer<InputState, ProblemDetailsError>, IPreUpdateChecker
{
    private readonly ILogger _logger;

    public PreUpdateChecker(IEnumerable<IPreUpdateCheck> preCheckHandlers, ILogger<PreUpdateChecker> logger)
        : base(preCheckHandlers.Cast<PreUpdateCheckBase>())
    {
        _logger = logger.WhenNotNull();
    }

    public Task<ProblemDetailsError?> CanSaveAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var state = new InputState
        {
            Input = input,
            RoleIds = input.RoleIds
        };

        return HandleAsync(state, cancellationToken);
    }
}

