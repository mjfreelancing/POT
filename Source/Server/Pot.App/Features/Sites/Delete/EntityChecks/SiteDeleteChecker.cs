using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.ChainOfResponsibility;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Sites.Delete.EntityChecks.Checks;
using Pot.App.Features.Sites.Delete.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Sites.Delete.EntityChecks;

/// <summary>
/// Default implementation of <see cref="ISiteDeleteChecker"/>.
/// </summary>
/// <remarks>
/// <para>
/// The checks are ordered deliberately, because the first failure is the only one reported. Ordering is imposed
/// here by <see cref="SiteDeleteCheckBase.Order"/> rather than left to injection order, which is not guaranteed.
/// </para>
/// <para>
/// The password and the two typed values run first, so a caller who has not completed the confirmation is told
/// exactly which value is wrong. <see cref="Checks.CheckSiteHasNoPlatformAdmin" /> runs last, because it is a
/// rule about the environment rather than about the caller's input.
/// </para>
/// </remarks>
internal sealed class SiteDeleteChecker : ChainOfResponsibilityAsyncComposer<InputState, ApiDetailError>, ISiteDeleteChecker
{
    private readonly ILogger _logger;

    /// <summary>
    /// Creates the checker from the available rules, ordered by <see cref="SiteDeleteCheckBase.Order"/>.
    /// </summary>
    /// <param name="deleteChecks">The deletion rules registered in the container.</param>
    /// <param name="logger">The logger.</param>
    public SiteDeleteChecker(IEnumerable<ISiteDeleteCheck> deleteChecks, ILogger<SiteDeleteChecker> logger)
        : base(deleteChecks.Cast<SiteDeleteCheckBase>().OrderBy(deleteCheck => deleteCheck.Order))
    {
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    public Task<ApiDetailError?> CanDeleteAsync(Input input, UserEntity caller, SiteEntity site, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var state = new InputState
        {
            Caller = caller,
            Site = site,
            SiteName = input.SiteName,
            Username = input.Username,
            Password = input.Password,
            PlatformAdminRowIds = input.PlatformAdminRowIds
        };

        return HandleAsync(state, cancellationToken);
    }
}
