namespace Pot.AspNetCore.Integration.Tests.Host.Models;

/// <summary>
/// The expense fields the integration fixtures assert against, as the read endpoints return them.
/// </summary>
/// <remarks>
/// Only the asserted fields are declared; the endpoints return more, which deserialization ignores.
/// </remarks>
public sealed class ExpenseResponse
{
    /// <summary>
    /// The expense identifier.
    /// </summary>
    public Guid RowId { get; set; }

    /// <summary>
    /// The entity tag an update must match.
    /// </summary>
    public long Etag { get; set; }

    /// <summary>
    /// The expense amount.
    /// </summary>
    public double Amount { get; set; }

    /// <summary>
    /// When the expense is next due.
    /// </summary>
    public DateOnly NextDue { get; set; }

    /// <summary>
    /// When automatic allocations begin accruing, if they do.
    /// </summary>
    public DateOnly? AccrualStart { get; set; }

    /// <summary>
    /// The amount accrued for the cycle in progress.
    /// </summary>
    public double Accrued { get; set; }

    /// <summary>
    /// The amount owed for occurrences that are already past due.
    /// </summary>
    public double Arrears { get; set; }
}
