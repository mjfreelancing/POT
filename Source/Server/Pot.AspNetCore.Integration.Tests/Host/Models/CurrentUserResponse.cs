namespace Pot.AspNetCore.Integration.Tests.Host.Models;

/// <summary>
/// The current-user fields the integration fixtures assert against, as the endpoint returns them.
/// </summary>
/// <remarks>
/// Only the asserted fields are declared; the endpoint returns more, which deserialization ignores.
/// </remarks>
public sealed class CurrentUserResponse
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
    /// The site the user belongs to.
    /// </summary>
    public CurrentUserSiteResponse Site { get; set; } = new();
}

/// <summary>
/// The site fields the current-user endpoint returns for the caller's site.
/// </summary>
public sealed class CurrentUserSiteResponse
{
    /// <summary>
    /// The site identifier.
    /// </summary>
    public Guid RowId { get; set; }

    /// <summary>
    /// The entity tag an update to the site must match.
    /// </summary>
    public long Etag { get; set; }

    /// <summary>
    /// The site name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The site description.
    /// </summary>
    public string? Description { get; set; }
}
