using AllOverIt.Patterns.Result;
using Pot.App.Features.Expenses.Update.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Expenses.Update;

/// <summary>
/// Updates an expense for the current site.
/// </summary>
/// <remarks>
/// The expense must exist and its persisted concurrency token must match the input before any changes are applied.
/// </remarks>
public interface IUpdateExpenseService : IPotScopedDependency
{
    /// <summary>
    /// Updates the expense identified by the input.
    /// </summary>
    /// <param name="input">The expense values to apply and the account the expense belongs to.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <c>EnrichedResult&lt;Output&gt;</c> containing the updated expense, or a failure when the expense or
    /// account does not exist, the concurrency token does not match, or a pre-update check rejects the request.
    /// </returns>
    Task<EnrichedResult<Output>> UpdateExpenseAsync(Input input, CancellationToken cancellationToken);
}
