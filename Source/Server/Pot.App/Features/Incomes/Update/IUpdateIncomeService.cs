using AllOverIt.Patterns.Result;
using Pot.App.Features.Incomes.Update.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Incomes.Update;

/// <summary>
/// Updates an income for the current site.
/// </summary>
/// <remarks>
/// The income must exist and its persisted concurrency token must match the input before any changes are applied.
/// </remarks>
public interface IUpdateIncomeService : IPotScopedDependency
{
    /// <summary>
    /// Updates the income identified by the input.
    /// </summary>
    /// <param name="input">The income values to apply and the account the income belongs to.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <c>EnrichedResult&lt;Output&gt;</c> containing the updated income, or a failure when the income or account
    /// does not exist, the concurrency token does not match, or a pre-update check rejects the request.
    /// </returns>
    Task<EnrichedResult<Output>> UpdateIncomeAsync(Input input, CancellationToken cancellationToken);
}
