using System.ComponentModel;

namespace Pot.AspNetCore.Features.Accruals.AccrueExpenses;

public sealed class Request
{
    [Description("The Account Ids")]
    public Guid[] RowIds { get; init; } = [];
}
