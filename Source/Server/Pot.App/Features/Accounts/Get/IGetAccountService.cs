using AllOverIt.Patterns.Result;
using Pot.App.Features.Accounts.Get.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accounts.Get;

/// <summary>
/// Retrieves a single account together with its calculated accrual position and linked record counts.
/// </summary>
/// <remarks>
/// Accrual totals are derived on demand from the account's persisted expense schedules, so the result reflects
/// the account's current position rather than a stored snapshot.
/// </remarks>
public interface IGetAccountService : IPotScopedDependency
{
    /// <summary>
    /// Gets the account identified by <paramref name="accountId"/>, including its accrual totals and its linked
    /// expense and income counts.
    /// </summary>
    /// <param name="accountId">The external <c>RowId</c> of the account to retrieve.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <c>EnrichedResult&lt;Output&gt;</c> containing the account projection, or a failure with a not-found
    /// error when no account matches <paramref name="accountId"/>.
    /// </returns>
    Task<EnrichedResult<Output>> GetAccountWithLinkedCountsAsync(Guid accountId, CancellationToken cancellationToken);
}
