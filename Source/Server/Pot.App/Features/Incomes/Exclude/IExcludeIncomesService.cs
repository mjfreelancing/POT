using AllOverIt.Patterns.Result;
using Pot.App.Features.Incomes.Exclude.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Incomes.Exclude;

/// <summary>
/// Toggles whether one or more incomes are excluded from projections.
/// </summary>
public interface IExcludeIncomesService : IPotScopedDependency
{
    /// <summary>
    /// Toggles the exclusion state of the incomes identified by the input.
    /// </summary>
    /// <param name="input">The incomes whose exclusion state is to be toggled.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <c>EnrichedResult&lt;bool&gt;</c> containing <see langword="true"/> when the exclusion states were
    /// toggled, or a failure when one or more of the supplied incomes do not exist.
    /// </returns>
    Task<EnrichedResult<bool>> ToggleExclusionAsync(Input input, CancellationToken cancellationToken);
}
