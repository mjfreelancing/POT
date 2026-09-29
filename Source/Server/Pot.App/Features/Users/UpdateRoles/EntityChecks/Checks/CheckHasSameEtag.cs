using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;

namespace Pot.App.Features.Users.UpdateRoles.EntityChecks.Checks;

/// <summary>
/// Rejects a role change when the supplied eTag does not match the stored user.
/// </summary>
internal sealed class CheckHasSameEtag : PreUpdateCheckBase
{
    private readonly ILogger _logger;

    public CheckHasSameEtag(ILogger<CheckHasSameEtag> logger)
    {
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    public override async Task<ApiDetailError?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var input = state.Input;

        if (state.UserEtag != input.Etag)
        {
            return ApiDetailErrorFactory.CreateEtagConflict("User", input.Etag);
        }

        return await base.HandleAsync(state, cancellationToken);
    }
}

