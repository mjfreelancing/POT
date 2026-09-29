using AllOverIt.Patterns.Result;
using Pot.App.Features.Users.UpdateStatus.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Users.UpdateStatus;

/// <summary>
/// Changes the status of an existing user.
/// </summary>
public interface IUpdateUserStatusService : IPotScopedDependency
{
    /// <summary>
    /// Sets the status of the user identified by the input.
    /// </summary>
    /// <param name="input">The user to update and the status to apply.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <c>EnrichedResult&lt;Output&gt;</c> containing the updated user, or a failure when the user does not exist
    /// or the eTag does not match.
    /// </returns>
    Task<EnrichedResult<Output>> UpdateUserStatusAsync(Input input, CancellationToken cancellationToken);
}
