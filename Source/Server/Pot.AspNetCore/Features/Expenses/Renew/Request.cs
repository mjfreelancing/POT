using System.ComponentModel;

namespace Pot.AspNetCore.Features.Expenses.Renew;

public sealed class Request
{
    [Description("The Expense Ids.")]
    public Guid[] RowIds { get; init; } = [];
}
