using Pot.App.Features.Accounts.GetAll.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accounts.GetAll;

/// <summary>
/// Retrieves every account together with its calculated accrual position and linked record counts.
/// </summary>
/// <remarks>
/// Accrual totals are derived on demand from each account's persisted expense schedules, so the results reflect
/// the accounts' current positions rather than a stored snapshot.
/// </remarks>
public interface IGetAllAccountsService : IPotScopedDependency
{
    /// <summary>
    /// Gets all accounts, each including its accrual totals and its linked expense and income counts.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The account projections, ordered by the repository's established account ordering.</returns>
    Task<Output[]> GetAllAccountsAsync(CancellationToken cancellationToken);
}
