using System.ComponentModel;

namespace Pot.AspNetCore.Features.Incomes.ToggleExclude;

public sealed class Request
{
    [Description("The Incomes Ids.")]
    public Guid[] RowIds { get; init; } = [];
}
