namespace Pot.App.Features.Sites.Delete.Models;

/// <summary>
/// Carries the values needed to delete the caller's site.
/// </summary>
public sealed class Input
{
    /// <summary>Gets the site name, typed as confirmation.</summary>
    public required string SiteName { get; init; }

    /// <summary>Gets the caller's username, typed as confirmation.</summary>
    public required string Username { get; init; }

    /// <summary>Gets the caller's current password, entered as confirmation.</summary>
    public required string Password { get; init; }

    /// <summary>Gets the configured platform administrator identifiers.</summary>
    public required IReadOnlyList<Guid> PlatformAdminRowIds { get; init; }
}
