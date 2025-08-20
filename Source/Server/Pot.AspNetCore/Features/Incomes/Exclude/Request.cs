using System.ComponentModel;

namespace Pot.AspNetCore.Features.Incomes.Exclude;

public sealed class Request
{
    [Description("The Incomes Ids.")]
    public Guid[] RowIds { get; init; } = [];

    [Description("Whether to exclude the incomes from calculations.")]
    public bool? ExcludeFromCalcs { get; init; }
}
