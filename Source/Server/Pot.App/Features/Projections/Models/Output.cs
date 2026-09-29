namespace Pot.App.Features.Projections.Models;

/// <summary>
/// The financial projection for a forecast window: a per-account series plus the combined series.
/// </summary>
/// <remarks>
/// This is the result the projection service returns and the projections endpoint exposes. Every day in the window
/// that falls on or after the requested start date is published, in date order.
/// </remarks>
public sealed class Output
{
    /// <summary>The projected values for each account, in the order the accounts were read.</summary>
    public required AccountDailyFinancialProjection[] Accounts { get; init; }

    /// <summary>The combined projected values of every account.</summary>
    public required DateProjection[] Global { get; init; }
}
