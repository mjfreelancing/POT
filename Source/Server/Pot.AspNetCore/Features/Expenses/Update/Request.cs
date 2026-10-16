using Pot.Shared.Enumerations;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Expenses.Update;

/// <summary>Carries the values needed to update an expense.</summary>
public sealed class Request
{
    /// <summary>The Expense's entity tag</summary>
    [Description("The Expense's entity tag")]
    public long Etag { get; init; }

    /// <summary>Is the expense excluded from calculations such as accruals</summary>
    [Description("Is the expense excluded from calculations such as accruals")]
    public bool? ExcludeFromCalcs { get; init; }

    /// <summary>A description of the account</summary>
    [Description("A description of the account")]
    public required string Description { get; init; }

    /// <summary>When automatic allocations will begin accruing for this expense</summary>
    [Description("When automatic allocations will begin accruing for this expense")]
    public DateOnly? AccrualStart { get; init; }

    /// <summary>The date when the next Expense amount will be credited to the associated account</summary>
    [Description("The date when the next Expense amount will be credited to the associated account")]
    public DateOnly NextDue { get; init; }

    /// <summary>The inclusive date when the Expense source will no longer credit the associated account</summary>
    [Description("The inclusive date when the Expense source will no longer credit the associated account")]
    public DateOnly? EndDate { get; init; }

    /// <summary>The expense accrual policy</summary>
    [Description("The expense accrual policy")]
    public required AccrualPolicy AccrualPolicy { get; init; }                      // Deserialized via EnrichedEnumJsonConverter<AccrualPolicy>

    /// <summary>The frequency unit the associated account will be credited</summary>
    [Description("The frequency unit the associated account will be credited")]
    public required Frequency Frequency { get; init; }                              // Deserialized via EnrichedEnumJsonConverter<Frequency>

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
