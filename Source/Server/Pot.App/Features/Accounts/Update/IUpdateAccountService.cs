using AllOverIt.Patterns.Result;
using Pot.App.Features.Accounts.Update.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accounts.Update;

/// <summary>
/// Updates an existing account.
/// </summary>
/// <remarks>
/// The caller must supply the etag of the version it read so conflicting edits are rejected rather than
/// silently overwritten.
/// </remarks>
public interface IUpdateAccountService : IPotScopedDependency
{
    /// <summary>
    /// Updates the account identified by the input's row identifier.
    /// </summary>
    /// <param name="input">The account identity, concurrency etag, and values to persist.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <c>EnrichedResult&lt;Output&gt;</c> containing the updated account's row identifier and etag, or a
    /// failure when the account does not exist, the etag is stale, or the description is already used by
    /// another account in the current site.
    /// </returns>
    Task<EnrichedResult<Output>> UpdateAccountAsync(Input input, CancellationToken cancellationToken);
}
