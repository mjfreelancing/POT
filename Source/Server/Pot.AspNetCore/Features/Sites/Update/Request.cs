using System.ComponentModel;

namespace Pot.AspNetCore.Features.Sites.Update;

public sealed class Request
{
    [Description("The site's entity tag")]
    public long Etag { get; init; }

    [Description("The site's name")]
    public required string Name { get; init; }

    [Description("The site's description")]
    public string? Description { get; init; }
}
