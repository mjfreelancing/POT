using System.ComponentModel;

namespace Pot.AspNetCore.Features.Accounts.Update;

public sealed class Request
{
    [Description("The account identifier.")]
    public Guid RowId { get; init; }

    [Description("The account's entity tag.")]
    public long Etag { get; init; }

    [Description("Is the account excluded from calculations such as accruals.")]
    public bool? ExcludeFromCalcs { get; init; }

    [Description("The account BSB.")]
    public required string Bsb { get; init; }

    [Description("The account number.")]
    public required string Number { get; init; }

    [Description("A description of the account.")]
    public required string Description { get; init; }

    [Description("The account balance.")]
    public double Balance { get; init; }

    [Description("The minimum reserved amount.")]
    public double Reserved { get; init; }
}
