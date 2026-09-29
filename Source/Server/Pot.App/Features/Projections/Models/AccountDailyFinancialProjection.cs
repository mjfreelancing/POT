namespace Pot.App.Features.Projections.Models;

/// <summary>
/// The projected values for a single account across the forecast window.
/// </summary>
/// <remarks>
/// Each account contributes one of these to <see cref="Output.Accounts" />, alongside the combined series held in
/// <see cref="Output.Global" />.
/// </remarks>
public sealed class AccountDailyFinancialProjection
{
    /// <summary>The account identity, so a block can be attributed back to its account.</summary>
    public required Guid RowId { get; init; }

    /// <summary>The account description.</summary>
    public required string Description { get; init; }

    /// <summary>The projected values for each day of the window, in date order.</summary>
    public required DateProjection[] Dates { get; init; }
}
