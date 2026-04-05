using Pot.Shared.Enumerations;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Expenses.Create;

public sealed class Request
{
    [Description("A description of the Expense")]
    public required string Description { get; init; }

    [Description("When automatic allocations will begin accruing for this expense")]
    public DateOnly? AccrualStart { get; init; }

    [Description("The date when the next Expense will be due")]
    public DateOnly NextDue { get; init; }

    [Description("The inclusive date when the Expense source will no longer credit the associated account")]
    public DateOnly? EndDate { get; init; }

    [Description("The expense accrual policy")]
    public required AccrualPolicy AccrualPolicy { get; init; }                          // Deserialized via EnrichedEnumJsonConverter<AccrualPolicy>

    [Description("The frequency unit the associated account will be credited")]
    public required Frequency Frequency { get; init; }                                  // Deserialized via EnrichedEnumJsonConverter<Frequency>

    [Description("The frequency count the associated account will be credited")]
    public int FrequencyCount { get; init; }

    [Description("The Expense amount")]
    public double Amount { get; init; }

    [Description("The identifier for the associated account to be credited")]
    public Guid AccountRowId { get; init; }

    [Description("A note about the expense")]
    public string? Note { get; init; }
}
