using System.ComponentModel;

namespace Pot.AspNetCore.Features.Settings.Upsert;

/// <summary>Carries the values needed to save settings.</summary>
public sealed class Request
{
    /// <summary>The setting value</summary>
    [Description("The setting value")]
    public required string Value { get; init; }

    /// <summary>The setting's entity tag (null for new settings)</summary>
    [Description("The setting's entity tag (null for new settings)")]
    public long? Etag { get; init; }
}
