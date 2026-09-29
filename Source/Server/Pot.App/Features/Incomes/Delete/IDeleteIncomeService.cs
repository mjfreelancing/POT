using AllOverIt.Patterns.Result;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Incomes.Delete;

/// <summary>
/// Deletes an income for the current site.
/// </summary>
/// <remarks>
/// A missing income is not treated as a failure; the call succeeds with <see langword="false"/>.
/// </remarks>
public interface IDeleteIncomeService : IPotScopedDependency
{
    /// <summary>
    /// Deletes the income identified by the supplied identifier.
    /// </summary>
    /// <param name="incomeId">The identifier of the income to delete.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <c>EnrichedResult&lt;bool&gt;</c> containing <see langword="true"/> when the income was deleted, or
    /// <see langword="false"/> when the income does not exist.
    /// </returns>
    Task<EnrichedResult<bool>> DeleteIncomeAsync(Guid incomeId, CancellationToken cancellationToken);
}
