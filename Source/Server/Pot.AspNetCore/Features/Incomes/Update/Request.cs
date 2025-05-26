using Pot.Data.Models;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Incomes.Update;

public sealed class Request
{
    [Description("The income identifier.")]
    public Guid RowId { get; init; }

    [Description("The income's entity tag.")]
    public long ETag { get; init; }

    [Description("A description of the account.")]
    public string Description { get; init; } = string.Empty;

    [Description("The date when the next income amount will be credited to the associated account.")]
    public DateOnly NextDue { get; init; }

    [Description("The inclusive date when the income source will no longer credit the associated account.")]
    public DateOnly? EndDate { get; init; }

    [Description("The frequency unit the associated account will be credited.")]
    public Frequency Frequency { get; init; } = Frequency.Months;

    [Description("The frequency count the associated account will be credited.")]
    public int FrequencyCount { get; init; }

    [Description("The income amount.")]
    public double Amount { get; init; }

    [Description("The identifier for the associated account to be credited.")]
    public Guid AccountRowId { get; init; }
}
