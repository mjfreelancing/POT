using AllOverIt.Patterns.Result;
using Pot.App.Features.Expenses.Get.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Expenses.Get;

/// <summary>
/// Retrieves an expense for the current site.
/// </summary>
public interface IGetExpenseService : IPotScopedDependency
{
    /// <summary>
    /// Gets the expense identified by the supplied identifier.
    /// </summary>
    /// <param name="expenseId">The identifier of the expense to retrieve.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <c>EnrichedResult&lt;Output&gt;</c> containing the expense and its accrual detail, or a failure when the
    /// expense does not exist.
    /// </returns>
    Task<EnrichedResult<Output>> GetExpenseAsync(Guid expenseId, CancellationToken cancellationToken);
}
