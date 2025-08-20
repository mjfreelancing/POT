using System.ComponentModel;

namespace Pot.AspNetCore.Features.Expenses.Exclude;

public sealed class Request
{
    [Description("The Expense Ids.")]
    public Guid[] RowIds { get; init; } = [];

    [Description("Whether to exclude the expenses from calculations.")]
    public bool? ExcludeFromCalcs { get; init; }
}
