using AllOverIt.Patterns.Result;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Expenses.Delete;

/// <summary>
/// Deletes an expense for the current site.
/// </summary>
public interface IDeleteExpenseService : IPotScopedDependency
{
    /// <summary>
    /// Deletes the expense identified by the supplied identifier.
    /// </summary>
    /// <param name="expenseId">The identifier of the expense to delete.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <c>EnrichedResult&lt;bool&gt;</c> containing <see langword="true"/> when the expense was deleted, or a
    /// failure when the expense does not exist.
    /// </returns>
    Task<EnrichedResult<bool>> DeleteExpenseAsync(Guid expenseId, CancellationToken cancellationToken);
}
