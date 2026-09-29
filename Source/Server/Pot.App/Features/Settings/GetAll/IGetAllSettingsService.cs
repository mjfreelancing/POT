using AllOverIt.Patterns.Result;
using Pot.App.Features.Settings.GetAll.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Settings.GetAll;

/// <summary>
/// Retrieves all settings for the current site, grouped by category.
/// </summary>
public interface IGetAllSettingsService : IPotScopedDependency
{
    /// <summary>
    /// Gets all settings for the current site, grouped by category and merged with the defaults for each category.
    /// </summary>
    /// <remarks>
    /// Settings that are not stored for the current site are returned with their default value and no row identifier
    /// or eTag, so they cannot be used to update an existing row.
    /// </remarks>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>An <c>EnrichedResult&lt;Output&gt;</c> containing the settings of every category.</returns>
    Task<EnrichedResult<Output>> GetAllSettingsAsync(CancellationToken cancellationToken);
}
