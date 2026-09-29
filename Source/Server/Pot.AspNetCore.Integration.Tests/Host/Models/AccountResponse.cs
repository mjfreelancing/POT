namespace Pot.AspNetCore.Integration.Tests.Host.Models;

/// <summary>
/// The account fields the integration fixtures assert against, as the read endpoints return them.
/// </summary>
/// <remarks>
/// Only the asserted fields are declared; the endpoints return more, which deserialization ignores.
/// </remarks>
public sealed class AccountResponse
{
    /// <summary>
    /// The account identifier.
    /// </summary>
    public Guid RowId { get; set; }

    /// <summary>
    /// The account balance.
    /// </summary>
    public double Balance { get; set; }

    /// <summary>
    /// The minimum reserved amount.
    /// </summary>
    public double Reserved { get; set; }

    /// <summary>
    /// The total amount accrued towards future expenses.
    /// </summary>
    public double TotalExpenseAccrued { get; set; }

    /// <summary>
    /// The total amount owed for expense cycles that are already past due.
    /// </summary>
    public double TotalArrears { get; set; }

    /// <summary>
    /// The accrued cycles plus the past-due arrears.
    /// </summary>
    public double TotalCommitted { get; set; }

    /// <summary>
    /// The balance after considering the reserved and committed amounts.
    /// </summary>
    public double Available { get; set; }
}
