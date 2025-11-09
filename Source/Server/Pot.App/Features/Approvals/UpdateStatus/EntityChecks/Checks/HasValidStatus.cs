using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Approvals.UpdateStatus.Models;
using Pot.Shared.Enumerations;

namespace Pot.App.Features.Approvals.UpdateStatus.EntityChecks.Checks;

internal sealed class HasValidStatus : PreUpdateCheckBase
{
    private readonly ILogger _logger;

    public HasValidStatus(ILogger<HasValidStatus> logger)
    {
        _logger = logger.WhenNotNull();
    }

    public override async Task<ProblemDetailsError?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var input = state.Input;
        var userToUpdate = state.UserToUpdate;

        if (userToUpdate.Status != UserStatus.Approval)
        {
            return ProblemDetailsErrorFactory.CreateUnprocessableEntityError(nameof(Input.Status), input.Status, "The user is not in a Pending Approval status");
        }

        return await base.HandleAsync(state, cancellationToken);
    }
}
