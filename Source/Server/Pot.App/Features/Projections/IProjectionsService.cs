using AllOverIt.Patterns.Result;
using Pot.App.Features.Projections.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Projections;

/// <summary>
/// Calculates financial projections across a set of accounts over a forward-looking window.
/// </summary>
/// <remarks>
/// The calculation is read-only: it walks the persisted schedules forward from site-local today without mutating
/// any entity, and returns both per-account and combined daily values for the requested window.
/// </remarks>
public interface IProjectionsService : IPotScopedDependency
{
    /// <summary>
    /// Gets the daily financial projections for the window described by the options.
    /// </summary>
    /// <param name="options">The window to project, including the first published date and the number of days.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <c>EnrichedResult&lt;Output&gt;</c> containing the combined and per-account daily projections. The
    /// failure outcome is not used, as an unsupported window is reported by throwing instead.
    /// </returns>
    Task<EnrichedResult<Output>> GetFinancialProjectionsAsync(ProjectionOptions options, CancellationToken cancellationToken);
}
