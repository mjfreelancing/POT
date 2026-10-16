using Pot.Shared.Enumerations;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Expenses.Renew;

/// <summary>Carries the values needed to renew an expense.</summary>
public sealed class Request
{
    /// <summary>The Expense Ids</summary>
    [Description("The Expense Ids")]
    public IReadOnlyList<Guid> RowIds { get; init; } = [];

    /// <summary>The reference date for the renewal operation (typically today's date)</summary>
    [Description("The reference date for the renewal operation (typically today's date)")]
    public required DateOnly AsOfDate { get; init; }

    /// <summary>Indicates the renewal mode to be applied</summary>
    [Description("Indicates the renewal mode to be applied")]
    public required RenewalMode Mode { get; init; }
}
