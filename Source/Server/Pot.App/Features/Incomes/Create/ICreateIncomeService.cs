using AllOverIt.Patterns.Result;
using Pot.App.Features.Incomes.Create.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Incomes.Create;

/// <summary>
/// Creates an income for the current site.
/// </summary>
/// <remarks>
/// The income is validated by the pre-create checks before it is attached to its account and persisted, so a
/// rejected request leaves no change behind.
/// </remarks>
public interface ICreateIncomeService : IPotScopedDependency
{
    /// <summary>
    /// Creates an income under the account specified by the input.
    /// </summary>
    /// <param name="input">The income to create and the account it belongs to.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <c>EnrichedResult&lt;Output&gt;</c> containing the created income, or a failure when the account does not
    /// exist or a pre-create check rejects the request.
    /// </returns>
    Task<EnrichedResult<Output>> CreateIncomeAsync(Input input, CancellationToken cancellationToken);
}
