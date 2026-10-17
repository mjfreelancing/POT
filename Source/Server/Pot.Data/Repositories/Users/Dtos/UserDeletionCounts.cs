namespace Pot.Data.Repositories.Users.Dtos;

/// <summary>
/// The rows removed when a user is deleted.
/// </summary>
/// <param name="Sessions">The number of authentication sessions removed.</param>
/// <param name="OneTimePasswords">The number of one-time codes removed.</param>
public sealed record UserDeletionCounts(int Sessions, int OneTimePasswords);
