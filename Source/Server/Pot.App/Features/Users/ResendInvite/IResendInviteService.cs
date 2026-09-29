using AllOverIt.Patterns.Result;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Users.ResendInvite;

/// <summary>
/// Resends a pending user invitation.
/// </summary>
public interface IResendInviteService : IPotScopedDependency
{
    /// <summary>
    /// Resets the user's password and re-queues the invitation email containing the new temporary password.
    /// </summary>
    /// <remarks>
    /// The existing password hash cannot be recovered, so a new temporary password is generated and the stored hash is
    /// replaced before the email is dispatched through the email outbox channel writer.
    /// </remarks>
    /// <param name="userRowId">The row identifier of the user whose invitation is being resent.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <c>EnrichedResult&lt;bool&gt;</c> that is successful once the invitation has been queued, or a failure when
    /// the user does not exist.
    /// </returns>
    Task<EnrichedResult<bool>> ResendInviteAsync(Guid userRowId, CancellationToken cancellationToken);
}
