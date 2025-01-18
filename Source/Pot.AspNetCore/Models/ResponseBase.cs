﻿using System.ComponentModel;

namespace Pot.AspNetCore.Models
{
    public abstract class ResponseBase
    {
        [Description("The resource identifier.")]
        public Guid RowId { get; init; }

        [Description("A tag indicating when the resource was created or updated.")]
        public long ETag { get; init; }
    }
}
