using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;

namespace Pot.App.Features.Approvals.UpdateStatus.EntityChecks.Checks;

/// <summary>
/// Rejects an approval update when the supplied etag does not match the user's current version.
/// </summary>
internal sealed class CheckHasSameEtag : PreUpdateCheckBase
{
    private readonly ILogger _logger;

    public CheckHasSameEtag(ILogger<CheckHasSameEtag> logger)
    {
        _logger = logger.WhenNotNull();
    }

    /// <summary>
    /// Fails when the user's etag differs from the etag supplied in the request, otherwise delegates to the next
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

        if (userToUpdate.Etag != input.Etag)
        {
            return ApiDetailErrorFactory.CreateEtagConflict("User", input.Etag);
        }

        return await base.HandleAsync(state, cancellationToken);
    }
}