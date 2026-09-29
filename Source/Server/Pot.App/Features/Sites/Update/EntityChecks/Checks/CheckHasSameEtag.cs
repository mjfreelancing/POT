using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;

namespace Pot.App.Features.Sites.Update.EntityChecks.Checks;

/// <summary>
/// Rejects a site update when the supplied eTag does not match the stored site.
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
        var site = state.SiteToUpdate;

        if (site.Etag != input.Etag)
        {
            return ApiDetailErrorFactory.CreateEtagConflict("Site", input.Etag);
        }

        return await base.HandleAsync(state, cancellationToken);
    }
}

