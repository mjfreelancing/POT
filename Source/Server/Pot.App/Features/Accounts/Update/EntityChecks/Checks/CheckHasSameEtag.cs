using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;

namespace Pot.App.Features.Accounts.Update.EntityChecks.Checks;

/// <summary>
/// Rejects an update when the supplied etag does not match the account's current version.
/// </summary>
internal sealed class CheckHasSameEtag : PreUpdateCheckBase
{
    private readonly ILogger _logger;

    public CheckHasSameEtag(ILogger<CheckHasSameEtag> logger)
    {
        _logger = logger.WhenNotNull();
    }

    /// <summary>
    /// Fails when the account's etag differs from the etag supplied in the update, otherwise delegates to the
    /// next check in the chain.
    /// </summary>
    /// <param name="state">The state carrying the update request and the account being validated.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The failing validation error, or <see langword="null"/> when the rule is satisfied.</returns>
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

