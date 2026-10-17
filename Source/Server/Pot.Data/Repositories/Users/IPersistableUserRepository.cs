using Pot.Data.Repositories.Users.Dtos;

namespace Pot.Data.Repositories.Users;

/// <summary>
/// Reads and writes users.
/// </summary>
public interface IPersistableUserRepository : IUserRepository, IPersistableRepository
{
    /// <summary>
    /// Removes a user in the caller's site together with their sessions and one-time codes.
    /// </summary>
    /// <remarks>
    /// The removal is ordered by the foreign keys: sessions, then one-time codes (those linked to the user and
    /// those carrying the username), then the user.
    /// </remarks>
    /// <param name="userRowId">The user to remove.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The removed counts, or <see langword="null" /> when the user is not in the caller's site.</returns>
    Task<UserDeletionCounts?> DeleteUserWithDependentsAsync(Guid userRowId, CancellationToken cancellationToken);
}
