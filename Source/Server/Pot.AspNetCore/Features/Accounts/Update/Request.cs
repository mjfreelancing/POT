using System.ComponentModel;

namespace Pot.AspNetCore.Features.Accounts.Update;

/// <summary>Carries the values needed to update an account.</summary>
public sealed class Request
{
    /// <summary>The account's entity tag</summary>
    [Description("The account's entity tag")]
    public long Etag { get; init; }

    /// <summary>A description of the account</summary>
    [Description("A description of the account")]
    public required string Description { get; init; }

    /// <summary>The account balance</summary>
    [Description("The account balance")]
    public double Balance { get; init; }

    /// <summary>The minimum reserved amount</summary>
    [Description("The minimum reserved amount")]
    public double Reserved { get; init; }
}
