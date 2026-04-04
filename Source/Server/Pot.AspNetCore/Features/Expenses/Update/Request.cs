using Pot.Shared.Enumerations;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Expenses.Update;

public sealed class Request
{
    [Description("The Expense's entity tag")]
    public long Etag { get; init; }

    [Description("Is the expense excluded from calculations such as accruals")]
    public bool? ExcludeFromCalcs { get; init; }

    [Description("A description of the account")]
    public required string Description { get; init; }

    [Description("When automatic allocations will begin accruing for this expense")]
    public DateOnly? AccrualStart { get; init; }

    [Description("The date when the next Expense amount will be credited to the associated account")]
    public DateOnly NextDue { get; init; }

    [Description("The inclusive date when the Expense source will no longer credit the associated account")]
    public DateOnly? EndDate { get; init; }

    [Description("The frequency unit the associated account will be credited")]
    public required Frequency Frequency { get; init; }

    [Description("The frequency count the associated account will be credited")]
    public int FrequencyCount { get; init; }

    [Description("The Expense amount")]
    public double Amount { get; init; }

    [Description("The identifier for the associated account to be credited")]
    public Guid AccountRowId { get; init; }

    [Description("A note about the expense")]
    public string? Note { get; init; }
}
