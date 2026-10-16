using System.ComponentModel;

namespace Pot.AspNetCore.Models;

/// <summary>Provides common fields for API response models.</summary>
public abstract class ResponseBase
{
    /// <summary>The resource identifier</summary>
    [Description("The resource identifier")]
    public Guid RowId { get; init; }

    /// <summary>The entity tag for optimistic concurrency</summary>
    [Description("The entity tag for optimistic concurrency")]
    public long Etag { get; init; }
}
