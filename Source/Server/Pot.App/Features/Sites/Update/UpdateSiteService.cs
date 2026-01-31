using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Extensions;
using Pot.App.Features.Sites.Update.EntityChecks;
using Pot.App.Features.Sites.Update.Mappings;
using Pot.App.Features.Sites.Update.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Sites;

namespace Pot.App.Features.Sites.Update;

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

    public async Task<EnrichedResult<Output>> UpdateSiteAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        using (_siteRepository.WithTracking())
        {
            var siteId = input.RowId;

            var siteToUpdate = await _siteRepository.Sites
                .SingleOrDefaultAsync(user => user.RowId == input.RowId, cancellationToken)
                .ConfigureAwait(false);

            if (siteToUpdate is null)
            {
                var siteNotFoundDetails = ProblemDetailsErrorFactory.CreateEntityNotFoundError(siteId, "The site does not exist");

                _logger.LogError(siteNotFoundDetails);

                return EnrichedResult.Fail<Output>(siteNotFoundDetails);
            }

            var problemDetails = await _preUpdateChecker
                .CanSaveAsync(input, siteToUpdate, cancellationToken)
                .ConfigureAwait(false);

            if (problemDetails is not null)
            {
                _logger.LogError(problemDetails);

                return EnrichedResult.Fail<Output>(problemDetails);
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
