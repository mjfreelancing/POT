using Pot.Shared;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Incomes.Update;

public sealed class Request
{
    [Description("The income's entity tag")]
    public long Etag { get; init; }

    [Description("Is the income excluded from calculations")]
    public bool? ExcludeFromCalcs { get; init; }

    [Description("A description of the account")]
    public required string Description { get; init; }

    [Description("The date when the next income amount will be credited to the associated account")]
    public DateOnly NextDue { get; init; }

    [Description("The inclusive date when the income source will no longer credit the associated account")]
    public DateOnly? EndDate { get; init; }

    [Description("The frequency unit the associated account will be credited")]
    public required Frequency Frequency { get; init; }

    [Description("The frequency count the associated account will be credited")]
    public int FrequencyCount { get; init; }

    [Description("The income amount")]
    public double Amount { get; init; }

    [Description("The identifier for the associated account to be credited")]
    public Guid AccountRowId { get; init; }

    [Description("A note about the income")]
    public string? Note { get; init; }
}
