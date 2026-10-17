using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Users.Delete.Models;

namespace Pot.App.Features.Users.Delete.EntityChecks.Checks;

/// <summary>
/// Rejects a deletion when the caller is the target.
/// </summary>
/// <remarks>
/// Removing your own account is a different operation, so this request must never be used for it. The message
/// says only that, deliberately: the server does not know which client surface the request came from, and the
/// caller has no supported reason to reach this state.
/// </remarks>
internal sealed class CheckTargetIsNotSelf : UserDeleteCheckBase
{
    private const string RefusalMessage = "You cannot delete your own account with this request.";

    private readonly ILogger _logger;

    public CheckTargetIsNotSelf(ILogger<CheckTargetIsNotSelf> logger)
    {
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    public override int Order => 1;

    /// <summary>
    /// Fails when the target is the caller, otherwise delegates to the next check.
    /// </summary>
    /// <param name="state">The caller and the target.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The failing validation error, or <see langword="null" /> when the rule is satisfied.</returns>
    public override async Task<ApiDetailError?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        if (state.Caller.RowId == state.Target.RowId)
        {
            return ApiDetailErrorFactory.CreateBadRequestError(
                nameof(Input.RowId),
                state.Target.RowId,
                RefusalMessage);
        }

        return await base.HandleAsync(state, cancellationToken);
    }
}
