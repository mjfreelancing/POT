using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Sites.Delete.Models;

namespace Pot.App.Features.Sites.Delete.EntityChecks.Checks;

/// <summary>
/// Rejects a site deletion when the typed site name is not the caller's site.
/// </summary>
/// <remarks>
/// The typed value exists to make the caller stop and read what they are doing; it is compared trimmed and
/// case-insensitively, matching the case-insensitive site names the database stores.
/// </remarks>
internal sealed class CheckSiteNameMatches : SiteDeleteCheckBase
{
    private readonly ILogger _logger;

    public CheckSiteNameMatches(ILogger<CheckSiteNameMatches> logger)
    {
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    public override int Order => 1;

    /// <summary>
    /// Fails when the typed site name is not the caller's site, otherwise delegates to the next check.
    /// </summary>
    /// <param name="state">The caller's site and the typed site name.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The failing validation error, or <see langword="null" /> when the rule is satisfied.</returns>
    public override async Task<ApiDetailError?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var matches = string.Equals(
            state.SiteName.Trim(),
            state.Site.Name.Trim(),
            StringComparison.OrdinalIgnoreCase);

        if (!matches)
        {
            return ApiDetailErrorFactory.CreateBadRequestError(
                nameof(Input.SiteName),
                "The site name does not match your site.");
        }

        return await base.HandleAsync(state, cancellationToken);
    }
}
