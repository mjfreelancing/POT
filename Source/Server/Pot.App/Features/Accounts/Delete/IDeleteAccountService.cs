using AllOverIt.Patterns.Result;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accounts.Delete;

/// <summary>
/// Deletes an account.
/// </summary>
/// <remarks>
/// An account can only be deleted when it has no linked expenses or income records.
/// </remarks>
public interface IDeleteAccountService : IPotScopedDependency
{
    /// <summary>
    /// Deletes the account identified by <paramref name="accountId"/>.
    /// </summary>
    /// <param name="accountId">The external <c>RowId</c> of the account to delete.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <c>EnrichedResult&lt;bool&gt;</c> containing <see langword="true"/> when the account was deleted, or a
    /// failure when the account does not exist or still has linked expenses or incomes.
    /// </returns>
    Task<EnrichedResult<bool>> DeleteAccountAsync(Guid accountId, CancellationToken cancellationToken);
}
