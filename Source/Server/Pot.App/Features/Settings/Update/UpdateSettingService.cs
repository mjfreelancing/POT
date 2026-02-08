using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Extensions;
using Pot.App.Features.Settings.Update.EntityChecks;
using Pot.App.Features.Settings.Update.Mappings;
using Pot.App.Features.Settings.Update.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Settings;
using Pot.Data.Repositories.Sites;

namespace Pot.App.Features.Settings.Update;

internal sealed class UpdateSettingService : IUpdateSettingService
{
    private readonly IPersistableSettingsRepository _settingsRepository;
    private readonly ISiteRepository _siteRepository;
    private readonly IPreUpdateChecker _preUpdateChecker;
    private readonly ILogger _logger;

    public UpdateSettingService(IPersistableSettingsRepository settingsRepository, ISiteRepository siteRepository,
        IPreUpdateChecker preUpdateChecker, ILogger<UpdateSettingService> logger)
    {
        _settingsRepository = settingsRepository.WhenNotNull();
        _siteRepository = siteRepository.WhenNotNull();
        _preUpdateChecker = preUpdateChecker.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<Output>> UpdateSettingAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        using (_settingsRepository.WithTracking())
        {
            var settingToUpdate = await _settingsRepository
                .GetSettingAsync(input.Category, input.Key, cancellationToken)
                .ConfigureAwait(false);

            // This includes checking for when:
            // - A setting was not found but the user provided an etag
            // - A setting was found but the input etag (which can be null)) doesn't match the setting's
            var problemDetails = await _preUpdateChecker
                .CanSaveAsync(input, settingToUpdate, cancellationToken)
                .ConfigureAwait(false);

            if (problemDetails is not null)
            {
                _logger.LogError(problemDetails);

                return EnrichedResult.Fail<Output>(problemDetails);
            }

            if (settingToUpdate is null)
            {
                // Create new setting
                var currentSite = _siteRepository.GetCurrentSite();

                settingToUpdate = new SettingEntity
                {
                    Site = currentSite,
                    Category = input.Category,
                    Key = input.Key,
                    Value = input.Value
                };

                _settingsRepository.Add(settingToUpdate);
            }
            else
            {
                // Update existing setting
                settingToUpdate.Value = input.Value;
            }

            await _settingsRepository
                .SaveAsync(cancellationToken)
                .ConfigureAwait(false);

            var output = settingToUpdate.MapToOutput();

            return EnrichedResult.Success(output);
        }
    }
}
