namespace Pot.App.Features.Users.Delete.Models;

/// <summary>
/// Carries the values needed to delete a user in the caller's site.
/// </summary>
public sealed class Input
{
    /// <summary>Gets the user to delete.</summary>
    public Guid RowId { get; init; }

    /// <summary>Gets the caller's current password, entered as confirmation.</summary>
    public required string Password { get; init; }

    /// <summary>Gets the configured platform administrator identifiers.</summary>
    public required IReadOnlyList<Guid> PlatformAdminRowIds { get; init; }
}
