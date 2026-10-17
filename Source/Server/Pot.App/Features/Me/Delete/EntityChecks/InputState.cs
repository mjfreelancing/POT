using Pot.Data.Entities;

namespace Pot.App.Features.Me.Delete.EntityChecks;

/// <summary>
/// Carries the values evaluated by the pre-delete checks during an account self-deletion.
/// </summary>
internal sealed class InputState
{
    /// <summary>Gets the signed-in caller.</summary>
    public required UserEntity Caller { get; init; }

    /// <summary>Gets the username the caller typed as confirmation.</summary>
    public required string Username { get; init; }

    /// <summary>Gets the caller's current password, entered as confirmation.</summary>
    public required string Password { get; init; }

    /// <summary>Gets the configured platform administrator identifiers.</summary>
    public required IReadOnlyList<Guid> PlatformAdminRowIds { get; init; }
}
