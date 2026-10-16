using System.ComponentModel;

namespace Pot.AspNetCore.Features.Expenses.ToggleExclude;

/// <summary>Carries the values needed to change the exclusion setting for an expense.</summary>
public sealed class Request
{
    /// <summary>The Expense Ids.</summary>
    [Description("The Expense Ids.")]
    public Guid[] RowIds { get; init; } = [];
}
