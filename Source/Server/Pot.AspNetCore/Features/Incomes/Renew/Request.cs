using System.ComponentModel;

namespace Pot.AspNetCore.Features.Incomes.Renew;

public sealed class Request
{
    [Description("The Income Ids.")]
    public Guid[] RowIds { get; init; } = [];
}
