using System.ComponentModel;

namespace Pot.AspNetCore.Features.Incomes.ToggleExclude;

/// <summary>Carries the values needed to change the exclusion setting for an income.</summary>
public sealed class Request
{
    /// <summary>The Incomes Ids</summary>
    [Description("The Incomes Ids")]
    public Guid[] RowIds { get; init; } = [];
}
