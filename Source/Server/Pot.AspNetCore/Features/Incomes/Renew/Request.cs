using System.ComponentModel;

namespace Pot.AspNetCore.Features.Incomes.Renew;

public sealed class Request
{
    [Description("The Income Ids.")]
    public Guid[] RowIds { get; init; } = [];

    [Description("The incomes will be renewed until the next due date exceeds this date.")]
    public required DateOnly UntilDate { get; init; }
}
