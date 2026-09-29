using AllOverIt.Patterns.Result;
using Pot.App.Features.Users.Invite.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Users.Invite;

/// <summary>
/// Invites a new user to the current site.
/// </summary>
public interface IInviteUserService : IPotScopedDependency
{
    /// <summary>
    /// Creates a pending user for the current site, assigns the requested roles, and sends an invitation email
    /// containing a temporary password.
    /// </summary>
    /// <remarks>
    /// The invitation email is dispatched through the email outbox channel writer. The temporary password is generated
    /// server-side and is never returned to the caller.
    /// </remarks>
    /// <param name="input">The username, email address, and roles for the invited user.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <c>EnrichedResult&lt;bool&gt;</c> that is successful once the invitation has been queued, or a failure when
    /// the username is already in use or a requested role does not exist.
    /// </returns>
    Task<EnrichedResult<bool>> InviteUserAsync(Input input, CancellationToken cancellationToken);
}
