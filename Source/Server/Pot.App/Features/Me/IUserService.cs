using Pot.App.Features.Me.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Me;

/// <summary>
/// Provides profile information for a user.
/// </summary>
public interface IUserService : IPotScopedDependency
{
    /// <summary>
    /// Gets the profile of the user identified by the supplied row identifier.
    /// </summary>
    /// <param name="userId">The row identifier of the user to retrieve.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The user's profile, including their site, or <see langword="null"/> when no such user exists.</returns>
    Task<Output?> GetUserInfoAsync(Guid userId, CancellationToken cancellationToken);
}
