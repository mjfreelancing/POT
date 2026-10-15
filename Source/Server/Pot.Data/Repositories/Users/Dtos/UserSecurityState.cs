using Pot.Shared.Enumerations;

namespace Pot.Data.Repositories.Users.Dtos;

/// <summary>
/// The parts of a user the token pipeline needs to validate an access token.
/// </summary>
/// <remarks>
/// Read before any user context exists, so it is one of the deliberate cross-tenant reads: the token's
/// subject is the only thing known when it is looked up.
/// </remarks>
public sealed class UserSecurityState
{
    /// <summary>
    /// The version the user's access tokens must carry to remain valid.
    /// </summary>
    public int TokenVersion { get; init; }

    /// <summary>
    /// The user's current status.
    /// </summary>
    public required UserStatus Status { get; init; }
}
