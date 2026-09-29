using AllOverIt.Patterns.Result;
using Pot.App.Features.Users.UpdateRoles.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Users.UpdateRoles;

/// <summary>
/// Replaces the roles assigned to an existing user.
/// </summary>
public interface IUpdateUserRolesService : IPotScopedDependency
{
    /// <summary>
    /// Assigns the supplied roles to the user identified by the input, replacing any existing assignments.
    /// </summary>
    /// <remarks>
    /// The change is performed inside a transaction, and the user's concurrency eTag is verified before the roles are
    /// replaced.
    /// </remarks>
    /// <param name="input">The user to update and the roles to assign.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <c>EnrichedResult&lt;Output&gt;</c> containing the updated user, or a failure when the user does not exist,
    /// the eTag does not match, or a requested role does not exist.
    /// </returns>
    Task<EnrichedResult<Output>> UpdateUserRolesAsync(Input input, CancellationToken cancellationToken);
}
