using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;

namespace Pot.App.Features.Accounts.Update.EntityChecks.Checks;

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
        var account = state.AccountToUpdate;

        if (account.Etag != input.Etag)
        {
            return ApiDetailErrorFactory.CreateEtagConflict("Account", input.Etag);
        }

        return await base.HandleAsync(state, cancellationToken);
    }
}

