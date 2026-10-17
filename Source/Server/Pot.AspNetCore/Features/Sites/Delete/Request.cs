using System.ComponentModel;

namespace Pot.AspNetCore.Features.Sites.Delete;

/// <summary>Carries the values needed to delete the caller's site.</summary>
public sealed class Request
{
    /// <summary>The site name, typed as confirmation</summary>
    [Description("The site name, typed as confirmation")]
    public required string SiteName { get; init; }

    /// <summary>The caller's username, typed as confirmation</summary>
    [Description("The caller's username, typed as confirmation")]
    public required string Username { get; init; }

    /// <summary>The caller's current password, entered as confirmation</summary>
    [Description("The caller's current password, entered as confirmation")]
    public required string Password { get; init; }
}
