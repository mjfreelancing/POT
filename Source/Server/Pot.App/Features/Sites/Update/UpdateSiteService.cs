using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Extensions;
using Pot.App.Features.Sites.Update.EntityChecks;
using Pot.App.Features.Sites.Update.Mappings;
using Pot.App.Features.Sites.Update.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Sites;

namespace Pot.App.Features.Sites.Update;

/// <summary>
/// Default implementation of <see cref="IUpdateSiteService"/>.
/// </summary>
internal sealed class UpdateSiteService : IUpdateSiteService
{
    private readonly IPersistableSiteRepository _siteRepository;
    private readonly IPreUpdateChecker _preUpdateChecker;
    private readonly ILogger _logger;

    public UpdateSiteService(IPersistableSiteRepository siteRepository, IPreUpdateChecker preUpdateChecker,
        ILogger<UpdateSiteService> logger)
    {
        _siteRepository = siteRepository.WhenNotNull();
        _preUpdateChecker = preUpdateChecker.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    public async Task<EnrichedResult<Output>> UpdateSiteAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        using (_siteRepository.WithTracking())
        {
            var siteId = input.RowId;

            // A site is resolved from the caller's context only: supplying another site's id is
            // indistinguishable from supplying one that does not exist.
            var currentSite = _siteRepository.GetCurrentSite();

            var siteToUpdate = currentSite.RowId == siteId
                ? currentSite
                : null;

            if (siteToUpdate is null)
            {
                _logger.LogWarning(
                    "Refused an update of site {RequestedSiteRowId}: the caller belongs to site {CallerSiteRowId}",
                    siteId,
                    currentSite.RowId);

                var siteNotFoundError = ApiDetailErrorFactory.CreateEntityNotFoundError(siteId, "The site does not exist");

                _logger.LogApiError(siteNotFoundError);

                return EnrichedResult.Fail<Output>(siteNotFoundError);
            }

            var apiError = await _preUpdateChecker
                .CanSaveAsync(input, siteToUpdate, cancellationToken)
                .ConfigureAwait(false);

            if (apiError is not null)
            {
                _logger.LogApiError(apiError);

                return EnrichedResult.Fail<Output>(apiError);
            }

            UpdateSiteEntity(siteToUpdate, input);

            _ = await _siteRepository.SaveAsync(cancellationToken);

            var output = siteToUpdate.MapToOutput();

            return EnrichedResult.Success(output);
        }
    }

    private static void UpdateSiteEntity(SiteEntity siteToUpdate, Input input)
    {
        siteToUpdate.Name = input.Name;
        siteToUpdate.Description = input.Description.IsNullOrEmpty() ? null : input.Description;
    }
}
