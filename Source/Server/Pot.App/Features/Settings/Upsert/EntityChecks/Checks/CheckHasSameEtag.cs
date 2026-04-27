using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;

namespace Pot.App.Features.Settings.Upsert.EntityChecks.Checks;

internal sealed class CheckHasSameEtag : PreUpdateCheckBase
{
    private readonly ILogger _logger;

    public CheckHasSameEtag(ILogger<CheckHasSameEtag> logger)
    {
        _logger = logger.WhenNotNull();
    }

    public override async Task<ApiDetailError?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var input = state.Input;
        var setting = state.SettingToUpdate;

        // If setting doesn't exist, expect Etag = null (creating new)
        if (setting is null)
        {
            if (input.Etag is not null)
            {
                return ApiDetailErrorFactory.CreateEtagConflict("Setting", input.Etag.Value);
            }
        }
        else
        {
            // If setting exists, Etag must be provided and match
            if (input.Etag is null || setting.Etag != input.Etag.Value)
            {
                return ApiDetailErrorFactory.CreateEtagConflict("Setting", input.Etag ?? 0);
            }
        }

        return await base.HandleAsync(state, cancellationToken);
    }
}
