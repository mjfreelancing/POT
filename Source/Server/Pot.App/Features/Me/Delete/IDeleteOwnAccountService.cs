using AllOverIt.Patterns.Result;
using Pot.App.Features.Me.Delete.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Me.Delete;

/// <summary>
/// Deletes the signed-in user's own account.
/// </summary>
public interface IDeleteOwnAccountService : IPotScopedDependency
{
    /// <summary>
    /// Removes the caller together with their sessions and one-time codes, after the deletion rules and the
    /// confirmation values have been verified.
    /// </summary>
    /// <param name="input">The typed username, the confirmation password and the configured platform administrators.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A successful result, or the first refusal.</returns>
    Task<EnrichedResult<bool>> DeleteAccountAsync(Input input, CancellationToken cancellationToken);
}
