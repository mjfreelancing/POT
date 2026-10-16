using System.ComponentModel;

namespace Pot.AspNetCore.Features.Sites.Update;

/// <summary>Carries the values needed to update a site.</summary>
public sealed class Request
{
    /// <summary>The site's entity tag</summary>
    [Description("The site's entity tag")]
    public long Etag { get; init; }

    /// <summary>The site's name</summary>
    [Description("The site's name")]
    public required string Name { get; init; }

    /// <summary>The site's description</summary>
    [Description("The site's description")]
    public string? Description { get; init; }
}
