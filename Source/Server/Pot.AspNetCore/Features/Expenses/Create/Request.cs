using Pot.Shared.Enumerations;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Expenses.Create;

/// <summary>Carries the values needed to create an expense.</summary>
public sealed class Request
{
    /// <summary>A description of the Expense</summary>
    [Description("A description of the Expense")]
    public required string Description { get; init; }

    /// <summary>When automatic allocations will begin accruing for this expense</summary>
    [Description("When automatic allocations will begin accruing for this expense")]
    public DateOnly? AccrualStart { get; init; }

    /// <summary>The date when the next Expense will be due</summary>
    [Description("The date when the next Expense will be due")]
    public DateOnly NextDue { get; init; }

    /// <summary>The inclusive date when the Expense source will no longer credit the associated account</summary>
    [Description("The inclusive date when the Expense source will no longer credit the associated account")]
    public DateOnly? EndDate { get; init; }

    /// <summary>The expense accrual policy</summary>
    [Description("The expense accrual policy")]
    public required AccrualPolicy AccrualPolicy { get; init; }                          // Deserialized via EnrichedEnumJsonConverter<AccrualPolicy>

    /// <summary>The frequency unit the associated account will be credited</summary>
    [Description("The frequency unit the associated account will be credited")]
    public required Frequency Frequency { get; init; }                                  // Deserialized via EnrichedEnumJsonConverter<Frequency>

    /// <summary>The frequency count the associated account will be credited</summary>
    [Description("The frequency count the associated account will be credited")]
    public int FrequencyCount { get; init; }

    /// <summary>The Expense amount</summary>
    [Description("The Expense amount")]
    public double Amount { get; init; }

    /// <summary>The identifier for the associated account to be credited</summary>
    [Description("The identifier for the associated account to be credited")]
    public Guid AccountRowId { get; init; }

    /// <summary>A note about the expense</summary>
    [Description("A note about the expense")]
    public string? Note { get; init; }
}
