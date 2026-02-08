using System.ComponentModel;

namespace Pot.AspNetCore.Models;

public abstract class ResponseBase
{
    [Description("The resource identifier")]
    public Guid RowId { get; init; }

    [Description("The entity tag for optimistic concurrency")]
    public long Etag { get; init; }
}
