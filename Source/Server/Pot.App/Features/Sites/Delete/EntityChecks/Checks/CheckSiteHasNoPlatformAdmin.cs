using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Sites.Delete.Models;
using Pot.Data.Repositories.Users;

namespace Pot.App.Features.Sites.Delete.EntityChecks.Checks;

/// <summary>
/// Rejects a site deletion when the site holds the last live platform administrator.
/// </summary>
/// <remarks>
/// Deleting the site removes everyone in it. When one of those users is the only enabled platform
/// administrator left, the platform would be left with nobody able to operate it, so the site cannot be deleted
/// until another platform administrator is configured and redeployed. Platform administrators in the site who
/// are not enabled do not block the deletion, because they cannot operate the platform either.
/// </remarks>
internal sealed class CheckSiteHasNoPlatformAdmin : SiteDeleteCheckBase
{
    private const string RefusalMessage =
        "This site contains the last enabled platform administrator. Another platform administrator must be configured first.";

    private readonly IUserRepository _userRepository;
    private readonly ILogger _logger;

    public CheckSiteHasNoPlatformAdmin(IUserRepository userRepository, ILogger<CheckSiteHasNoPlatformAdmin> logger)
    {
        _userRepository = userRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    public override int Order => 3;

    /// <summary>
    /// Fails when the site holds the last enabled platform administrator, otherwise delegates to the next check.
    /// </summary>
    /// <param name="state">The caller's site and the configured platform administrators.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The failing validation error, or <see langword="null" /> when the rule is satisfied.</returns>
    public override async Task<ApiDetailError?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        if (await HoldsLastEnabledPlatformAdminAsync(state, cancellationToken))
        {
            return ApiDetailErrorFactory.CreateBadRequestError(
                nameof(Input.SiteName),
                state.Site.RowId,
                RefusalMessage);
        }

        return await base.HandleAsync(state, cancellationToken);
    }

    private async Task<bool> HoldsLastEnabledPlatformAdminAsync(InputState state, CancellationToken cancellationToken)
    {
        var siteUsers = await _userRepository.GetAllForCurrentSiteAsync(cancellationToken);
        var siteUserRowIds = siteUsers.Select(user => user.RowId).ToList();

        var configuredInSite = state.PlatformAdminRowIds
            .Where(siteUserRowIds.Contains)
            .ToList();

        if (configuredInSite.Count == 0)
        {
            return false;
        }

        var configuredOutsideSite = state.PlatformAdminRowIds
            .Where(rowId => !configuredInSite.Contains(rowId))
            .ToList();

        if (await _userRepository.CountEnabledPlatformAdminsAsync(configuredOutsideSite, cancellationToken) > 0)
        {
            return false;
        }

        // Nothing outside the site can operate the platform. The deletion is only blocked when somebody in the
        // site can: a configured identifier that belongs to a disabled or missing user is not a live operator.
        return await _userRepository.CountEnabledPlatformAdminsAsync(configuredInSite, cancellationToken) > 0;
    }
}
