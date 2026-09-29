using AllOverIt.Patterns.Result;
using Pot.App.Features.Users.Update.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Users.Update;

/// <summary>
/// Updates the profile details of an existing user.
/// </summary>
public interface IUpdateUserService : IPotScopedDependency
{
    /// <summary>
    /// Updates the display name and email address of the user identified by the input.
    /// </summary>
    /// <param name="input">The user to update and the values to apply.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <c>EnrichedResult&lt;Output&gt;</c> containing the updated user, or a failure when the user does not exist
    /// or the eTag does not match.
    /// </returns>
    Task<EnrichedResult<Output>> UpdateUserAsync(Input input, CancellationToken cancellationToken);
}
