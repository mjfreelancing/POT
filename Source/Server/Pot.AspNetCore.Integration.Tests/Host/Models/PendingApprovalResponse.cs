namespace Pot.AspNetCore.Integration.Tests.Host.Models;

/// <summary>
/// The pending-approval fields the integration fixtures assert against, as the approval list returns them.
/// </summary>
/// <remarks>
/// Only the asserted fields are declared; the endpoint returns more, which deserialization ignores.
/// </remarks>
public sealed class PendingApprovalResponse
{
    /// <summary>
    /// The identifier of the user awaiting approval.
    /// </summary>
    public Guid RowId { get; set; }

    /// <summary>
    /// The entity tag an approval decision must match.
    /// </summary>
    public long Etag { get; set; }

    /// <summary>
    /// The name the user signs in with.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// The user's email address.
    /// </summary>
    public string? Email { get; set; }
}
