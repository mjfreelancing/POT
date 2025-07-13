using Pot.Shared;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Expenses.Update;

public sealed class Request
{
    [Description("The Expense identifier.")]
    public Guid RowId { get; init; }

    [Description("The Expense's entity tag.")]
    public long Etag { get; init; }

    [Description("A description of the account.")]
    public string Description { get; init; } = string.Empty;

    [Description("When automatic allocations will begin accruing for this expense.")]
    public DateOnly AccrualStart { get; init; }

    [Description("The date when the next Expense amount will be credited to the associated account.")]
    public DateOnly NextDue { get; init; }

    [Description("The inclusive date when the Expense source will no longer credit the associated account.")]
    public DateOnly? EndDate { get; init; }

    [Description("The frequency unit the associated account will be credited.")]
    public Frequency Frequency { get; init; } = Frequency.Months;

    [Description("The frequency count the associated account will be credited.")]
    public int FrequencyCount { get; init; }

    [Description("The Expense amount.")]
    public double Amount { get; init; }

    [Description("The identifier for the associated account to be credited.")]
    public Guid AccountRowId { get; init; }

    [Description("A note about the expense")]
    public string? Note { get; init; }
}
