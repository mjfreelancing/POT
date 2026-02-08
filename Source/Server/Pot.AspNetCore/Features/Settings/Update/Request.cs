using System.ComponentModel;

namespace Pot.AspNetCore.Features.Settings.Update;

public sealed class Request
{
    [Description("The setting value")]
    public required string Value { get; init; }

    [Description("The setting's entity tag (null for new settings)")]
    public long? Etag { get; init; }
}
