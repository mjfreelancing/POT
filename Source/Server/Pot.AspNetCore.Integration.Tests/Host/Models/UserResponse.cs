namespace Pot.AspNetCore.Integration.Tests.Host.Models;

/// <summary>
/// The user fields the integration fixtures assert against, as the user list returns them.
/// </summary>
/// <remarks>
/// Only the asserted fields are declared; the endpoint returns more, which deserialization ignores.
/// </remarks>
public sealed class UserResponse
{
    /// <summary>
    /// The user identifier.
    /// </summary>
    public Guid RowId { get; set; }

    /// <summary>
    /// The entity tag an update to the user must match.
    /// </summary>
    public long Etag { get; set; }

    /// <summary>
    /// The name the user signs in with.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// The name shown for the user.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// The user's email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// The user's status, by name.
    /// </summary>
    public string? Status { get; set; }
}
