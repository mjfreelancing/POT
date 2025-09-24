using System.ComponentModel;

namespace Pot.AspNetCore.Features.Expenses.Renew;

public sealed class Request
{
    [Description("The Expense Ids")]
    public Guid[] RowIds { get; init; } = [];

    [Description("The expenses will be renewed to the next due date exceeds this date")]
    public required DateOnly UntilDate { get; init; }
}
