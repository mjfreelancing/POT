using AllOverIt.Patterns.Result;
using Pot.App.Features.Settings.Upsert.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Settings.Upsert;

/// <summary>
/// Creates or updates a single setting for the current site.
/// </summary>
public interface IUpsertSettingService : IPotScopedDependency
{
    /// <summary>
    /// Creates the setting when it does not exist, or updates it when it does, after validating its value.
    /// </summary>
    /// <remarks>
    /// New settings are associated with the current site, which is resolved from the request scope. The setting value
    /// is validated against the metadata registered for its category and key before it is persisted.
    /// </remarks>
    /// <param name="input">The setting to create or update.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <c>EnrichedResult&lt;Output&gt;</c> containing the saved setting, or a failure when the eTag does not match
    /// or the value is not valid for its category.
    /// </returns>
    Task<EnrichedResult<Output>> UpsertSettingAsync(Input input, CancellationToken cancellationToken);
}
