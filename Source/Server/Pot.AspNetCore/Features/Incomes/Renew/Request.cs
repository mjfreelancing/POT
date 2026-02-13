using Pot.Shared.Enumerations;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Incomes.Renew;

public sealed class Request
{
    [Description("The Income Ids")]
    public Guid[] RowIds { get; init; } = [];

    [Description("The reference date for the renewal operation (typically today's date)")]
    public required DateOnly AsOfDate { get; init; }

    [Description("Indicates the renewal mode to be applied")]
    public required RenewalMode Mode { get; init; }
}
