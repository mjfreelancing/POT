using Pot.Shared.Enumerations;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Incomes.Renew;

/// <summary>Carries the values needed to renew an income.</summary>
public sealed class Request
{
    /// <summary>The Income Ids</summary>
    [Description("The Income Ids")]
    public IReadOnlyList<Guid> RowIds { get; init; } = [];

    /// <summary>The reference date for the renewal operation (typically today's date)</summary>
    [Description("The reference date for the renewal operation (typically today's date)")]
    public required DateOnly AsOfDate { get; init; }

    /// <summary>Indicates the renewal mode to be applied</summary>
    [Description("Indicates the renewal mode to be applied")]
    public required RenewalMode Mode { get; init; }
}
