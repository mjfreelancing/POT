using Pot.Shared;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Expenses.Create;

public sealed class Request
{
    [Description("A description of the Expense.")]
    public string Description { get; init; } = string.Empty;

    [Description("The date when the next Expense will be due.")]
    public DateOnly NextDue { get; init; }

    [Description("The date when the next Expense amount will begin accruing.")]
    public DateOnly AccrualStart { get; init; }

    [Description("The inclusive date when the Expense source will no longer credit the associated account.")]
    public DateOnly? EndDate { get; init; }

    [Description("The frequency unit the associated account will be credited.")]
    public Frequency Frequency { get; init; } = Frequency.Months;

    [Description("The frequency count the associated account will be credited.")]
    public int FrequencyCount { get; init; }

    [Description("If the Expense is recurring.")]
    public bool Recurring { get; init; }

    [Description("The Expense amount.")]
    public double Amount { get; init; }

    [Description("The identifier for the associated account to be credited.")]
    public Guid AccountRowId { get; init; }
}
