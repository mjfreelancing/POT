using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Extensions;
using Pot.App.Features.Settings.Models;
using Pot.App.Features.Settings.Models.EmailBudgetReminder;
using Pot.App.Features.Settings.Update.EntityChecks;
using Pot.App.Features.Settings.Update.Mappings;
using Pot.App.Features.Settings.Update.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Settings;
using Pot.Data.Repositories.Sites;
using Pot.Shared.Enumerations;
using System.Diagnostics;

namespace Pot.App.Features.Settings.Update;

internal sealed class UpdateSettingService : IUpdateSettingService
{
    // Registry of setting validators mapped by category.
    // Each category maps to a generic validator function that delegates to the category-specific
    // validation implementation (via ISettingValueValidatable interface).
    // New setting categories must be registered here to enable validation.
    private static readonly Dictionary<SettingCategory, Func<string, string, ProblemDetailsError?>> SettingValueValidators = new()
    {
        [SettingCategory.EmailBudgetReminder] = ValidateSettingValue<EmailBudgetReminderSettings>
    };

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

            // Validates the setting value before persisting to the database.
            //
            // This validation ensures:
            //   1. The setting category has a registered validator (throws UnreachableException if not found)
            //   2. The setting key is recognized for the category (throws UnreachableException if not found)
            //   3. The value can be parsed to the correct type (e.g., "true" -> bool, "7" -> int)
            //   4. The value meets any range or business rule constraints (e.g., hour 0-23, days 0-31)
            // 
            // The validation is performed via a two-level lookup:
            //   - First level: SettingValueValidators dictionary maps category to a validator function
            //   - Second level: Category-specific validator (e.g., EmailBudgetReminderSettings) maps key to validation logic
            // 
            // Returns null if validation passes, ProblemDetailsError with details if validation fails
            var validationError = ValidateSettingValue(input.Category, input.Key, input.Value);

            if (validationError is not null)
            {
                _logger.LogError(validationError);

                return EnrichedResult.Fail<Output>(validationError);
            }

            // We can now crate (or update) the setting with confidence that the value is valid for the category and key.
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

    /// <summary>
    /// Validates a setting value for a specific category and key.
    /// This is the first-level lookup that routes validation to the appropriate category-specific validator.
    /// </summary>
    /// <param name="category">The setting category (e.g., EmailBudgetReminder)</param>
    /// <param name="keyName">The setting key name (e.g., "Enabled", "ReminderDays")</param>
    /// <param name="value">The string value to validate</param>
    /// <returns><see cref="ProblemDetailsError"/> if validation fails, <see langword="null"/> if validation passes</returns>
    /// <exception cref="UnreachableException">Thrown when no validator is registered for the category</exception>
    private static ProblemDetailsError? ValidateSettingValue(SettingCategory category, string keyName, string value)
    {
        var validator = SettingValueValidators.TryGetValue(category, out var validatorFunc)
            ? validatorFunc
            : throw new UnreachableException($"No setting value validator found for category '{category.Name}'");

        return validator.Invoke(keyName, value);
    }

    /// <summary>
    /// Generic validator that delegates to the category-specific validation implementation.
    /// This method acts as a bridge between the validator registry and the static interface method.
    /// </summary>
    /// <typeparam name="TSetting">The setting type that implements <see cref="ISettingValueValidatable"/> (e.g., EmailBudgetReminderSettings)</typeparam>
    /// <param name="keyName">The setting key name within the category</param>
    /// <param name="value">The string value to validate</param>
    /// <returns><see cref="ProblemDetailsError"/> if validation fails, <see langword="null"/> if validation passes</returns>
    private static ProblemDetailsError? ValidateSettingValue<TSetting>(string keyName, string value) where TSetting : ISettingValueValidatable
    {
        return TSetting.ValidateValue(keyName, value);
    }
}
