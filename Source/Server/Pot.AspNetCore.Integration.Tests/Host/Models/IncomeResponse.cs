namespace Pot.AspNetCore.Integration.Tests.Host.Models;

/// <summary>
/// The income fields the integration fixtures assert against, as the read endpoints return them.
/// </summary>
/// <remarks>
/// Only the asserted fields are declared; the endpoints return more, which deserialization ignores.
/// </remarks>
public sealed class IncomeResponse
{
    /// <summary>
    /// The account an income is credited to, as the read endpoints nest it.
    /// </summary>
    public sealed class AccountModel
    {
        /// <summary>The account identifier.</summary>
        public Guid RowId { get; set; }
    }

    /// <summary>
    /// The income identifier.
    /// </summary>
    public Guid RowId { get; set; }

    /// <summary>
    /// The entity tag an update must match.
    /// </summary>
    public long Etag { get; set; }

    /// <summary>
    /// Whether the income is excluded from calculations.
    /// </summary>
    public bool ExcludeFromCalcs { get; set; }

    /// <summary>
    /// A description of the income.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// When the income is next due.
    /// </summary>
    public DateOnly NextDue { get; set; }

    /// <summary>
    /// The income amount.
    /// </summary>
    public double Amount { get; set; }

    /// <summary>
    /// The account the income is credited to.
    /// </summary>
    public AccountModel? Account { get; set; }
}
