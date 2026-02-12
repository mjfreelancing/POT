using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.ChainOfResponsibility;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Sites.Update.EntityChecks.Checks;
using Pot.App.Features.Sites.Update.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Sites.Update.EntityChecks;

internal sealed class PreUpdateChecker : ChainOfResponsibilityAsyncComposer<InputState, ApiDetailError>, IPreUpdateChecker
{
    private readonly ILogger _logger;

    public PreUpdateChecker(IEnumerable<IPreUpdateCheck> preCheckHandlers, ILogger<PreUpdateChecker> logger)
        : base(preCheckHandlers.Cast<PreUpdateCheckBase>())
    {
        _logger = logger.WhenNotNull();
    }

    public Task<ApiDetailError?> CanSaveAsync(Input input, SiteEntity siteToUpdate, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var state = new InputState
        {
            Input = input,
            SiteToUpdate = siteToUpdate
        };

        return HandleAsync(state, cancellationToken);
    }
}

