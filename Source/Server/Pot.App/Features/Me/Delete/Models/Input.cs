namespace Pot.App.Features.Me.Delete.Models;

/// <summary>
/// Carries the values needed for a signed-in user to delete their own account.
/// </summary>
public sealed class Input
{
    /// <summary>Gets the caller's username, typed as confirmation.</summary>
    public required string Username { get; init; }

    /// <summary>Gets the caller's current password, entered as confirmation.</summary>
    public required string Password { get; init; }

    /// <summary>Gets the configured platform administrator identifiers.</summary>
    public required IReadOnlyList<Guid> PlatformAdminRowIds { get; init; }
}
