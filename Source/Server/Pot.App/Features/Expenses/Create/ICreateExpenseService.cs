using AllOverIt.Patterns.Result;
using Pot.App.Features.Expenses.Create.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Expenses.Create;

/// <summary>
/// Creates an expense for the current site.
/// </summary>
/// <remarks>
/// The expense is validated by the pre-create checks before it is attached to its account and persisted, so a
/// rejected request leaves no change behind.
/// </remarks>
public interface ICreateExpenseService : IPotScopedDependency
{
    /// <summary>
    /// Creates an expense under the account specified by the input.
    /// </summary>
    /// <param name="input">The expense to create and the account it belongs to.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <c>EnrichedResult&lt;Output&gt;</c> containing the created expense, or a failure when the account does
    /// not exist or a pre-create check rejects the request.
    /// </returns>
    Task<EnrichedResult<Output>> CreateExpenseAsync(Input input, CancellationToken cancellationToken);
}
