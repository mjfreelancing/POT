using System.ComponentModel;

namespace Pot.AspNetCore.Features.Users.Delete;

/// <summary>Carries the values needed to delete a user.</summary>
public sealed class Request
{
    /// <summary>The caller's current password, entered as confirmation</summary>
    [Description("The caller's current password, entered as confirmation")]
    public required string Password { get; init; }
}
