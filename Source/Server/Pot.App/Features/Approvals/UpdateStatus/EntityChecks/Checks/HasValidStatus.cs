using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Approvals.UpdateStatus.Models;
using Pot.Shared.Enumerations;

namespace Pot.App.Features.Approvals.UpdateStatus.EntityChecks.Checks;

/// <summary>
/// Rejects an approval update when the user is not awaiting approval.
/// </summary>
internal sealed class HasValidStatus : PreUpdateCheckBase
{
    private readonly ILogger _logger;

    public HasValidStatus(ILogger<HasValidStatus> logger)
    {
        _logger = logger.WhenNotNull();
    }

    /// <summary>
    /// Fails when the user is not currently in the pending approval status, otherwise delegates to the next
    /// check in the chain.
    /// </summary>
    /// <param name="state">The state carrying the approval request and the user being validated.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The failing validation error, or <see langword="null"/> when the rule is satisfied.</returns>
    public override async Task<ApiDetailError?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var input = state.Input;
        var userToUpdate = state.UserToUpdate;

        if (userToUpdate.Status != UserStatus.Approval)
        {
            return ApiDetailErrorFactory.CreateUnprocessableEntityError(nameof(Input.Status), input.Status, "The user is not in a Pending Approval status");
        }

        return await base.HandleAsync(state, cancellationToken);
    }
}
