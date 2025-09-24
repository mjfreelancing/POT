using System.ComponentModel;

namespace Pot.AspNetCore.Features.Accounts.AccrueExpenses;

public sealed class Request
{
    [Description("The Account Ids")]
    public Guid[] RowIds { get; init; } = [];
}
