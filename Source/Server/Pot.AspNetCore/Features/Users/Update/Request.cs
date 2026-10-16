using System.ComponentModel;

namespace Pot.AspNetCore.Features.Users.Update;

/// <summary>Carries the values needed to update a user.</summary>
public sealed class Request
{
    /// <summary>The users's entity tag</summary>
    [Description("The users's entity tag")]
    public long Etag { get; init; }

    /// <summary>The user's display name</summary>
    [Description("The user's display name")]
    public required string DisplayName { get; init; }

    /// <summary>The user's email</summary>
    [Description("The user's email")]
    public required string Email { get; init; }
}
