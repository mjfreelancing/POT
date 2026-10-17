using Pot.Data.Entities;

namespace Pot.App.Features.Users.Delete.EntityChecks;

/// <summary>
/// Carries the values evaluated by the pre-delete checks during a user deletion.
/// </summary>
internal sealed class InputState
{
    /// <summary>Gets the signed-in caller.</summary>
    public required UserEntity Caller { get; init; }

    /// <summary>
    /// Gets the user being deleted. It belongs to the caller's site, because the checks run only for a row the
    /// site-scoped lookup returned.
    /// </summary>
    public required UserEntity Target { get; init; }

    /// <summary>Gets the caller's current password, entered as confirmation.</summary>
    public required string Password { get; init; }

    /// <summary>Gets the configured platform administrator identifiers.</summary>
    public required IReadOnlyList<Guid> PlatformAdminRowIds { get; init; }
}
