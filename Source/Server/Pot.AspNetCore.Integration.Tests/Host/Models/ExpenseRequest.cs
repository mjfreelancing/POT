namespace Pot.AspNetCore.Integration.Tests.Host.Models;

/// <summary>
/// The create and update payload for an expense, shared so every fixture sends the same wire shape.
/// </summary>
/// <remarks>
/// Both endpoints bind these property names. <see cref="Etag" /> is only read by update, so sending it on create
/// is ignored; <see cref="ExcludeFromCalcs" /> is required by update and ignored by create. Dates and enumerations
/// are strings because that is the HTTP contract: dates are <c>yyyy-MM-dd</c> and the enumerations travel as their
/// names.
/// </remarks>
public sealed record ExpenseRequest
{
    /// <summary>
    /// The entity tag an update must match.
    /// </summary>
    public long Etag { get; init; }

    /// <summary>
    /// Whether the expense is excluded from calculations such as accruals. The update contract rejects a request
    /// that omits it.
    /// </summary>
    public bool ExcludeFromCalcs { get; init; }

    /// <summary>
    /// A description of the expense.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// When automatic allocations begin accruing, as <c>yyyy-MM-dd</c>.
    /// </summary>
    public string? AccrualStart { get; init; }

    /// <summary>
    /// When the expense is next due, as <c>yyyy-MM-dd</c>.
    /// </summary>
    public required string NextDue { get; init; }

    /// <summary>
    /// When the expense stops recurring, as <c>yyyy-MM-dd</c>.
    /// </summary>
    public string? EndDate { get; init; }

    /// <summary>
    /// The accrual policy name.
    /// </summary>
    public required string AccrualPolicy { get; init; }

    /// <summary>
    /// The frequency name.
    /// </summary>
    public required string Frequency { get; init; }

    /// <summary>
    /// The number of frequency units between occurrences.
    /// </summary>
    public int FrequencyCount { get; init; }

    /// <summary>
    /// The expense amount.
    /// </summary>
    public double Amount { get; init; }

    /// <summary>
    /// The account the expense belongs to.
    /// </summary>
    public Guid AccountRowId { get; init; }

    /// <summary>
    /// An optional note about the expense.
    /// </summary>
    public string? Note { get; init; }
}
