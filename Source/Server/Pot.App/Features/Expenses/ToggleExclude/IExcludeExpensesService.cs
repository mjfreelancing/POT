using AllOverIt.Patterns.Result;
using Pot.App.Features.Expenses.ToggleExclude.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Expenses.ToggleExclude;

/// <summary>
/// Toggles whether one or more expenses are excluded from projections.
/// </summary>
public interface IExcludeExpensesService : IPotScopedDependency
{
    /// <summary>
    /// Toggles the exclusion state of the expenses identified by the input.
    /// </summary>
    /// <param name="input">The expenses whose exclusion state is to be toggled.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <c>EnrichedResult&lt;bool&gt;</c> containing <see langword="true"/> when the exclusion states were
    /// toggled, or a failure when one or more of the supplied expenses do not exist.
    /// </returns>
    Task<EnrichedResult<bool>> ToggleExclusionAsync(Input input, CancellationToken cancellationToken);
}
