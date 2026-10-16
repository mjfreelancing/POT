using Pot.Shared.Enumerations;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Incomes.Create;

/// <summary>Carries the values needed to create an income.</summary>
public sealed class Request
{
    /// <summary>A description of the Income</summary>
    [Description("A description of the Income")]
    public string Description { get; init; } = string.Empty;

    /// <summary>The date when the next income amount will be credited to the associated account</summary>
    [Description("The date when the next income amount will be credited to the associated account")]
    public DateOnly NextDue { get; init; }

    /// <summary>The inclusive date when the income source will no longer credit the associated account</summary>
    [Description("The inclusive date when the income source will no longer credit the associated account")]
    public DateOnly? EndDate { get; init; }

    /// <summary>The frequency unit the associated account will be credited</summary>
    [Description("The frequency unit the associated account will be credited")]
    public required Frequency Frequency { get; init; }

    /// <summary>The frequency count the associated account will be credited</summary>
    [Description("The frequency count the associated account will be credited")]
    public int FrequencyCount { get; init; }

    /// <summary>The income amount</summary>
    [Description("The income amount")]
    public double Amount { get; init; }

    /// <summary>The identifier for the associated account to be credited</summary>
    [Description("The identifier for the associated account to be credited")]
    public Guid AccountRowId { get; init; }

    /// <summary>A note about the income</summary>
    [Description("A note about the income")]
    public string? Note { get; init; }
}
