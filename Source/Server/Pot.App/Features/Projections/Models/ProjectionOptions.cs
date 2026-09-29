namespace Pot.App.Features.Projections.Models;

/// <summary>
/// The window a financial projection is calculated over.
/// </summary>
/// <remarks>
/// The window always begins at site-local today so the running balance can be carried forward, but only the days
/// from <see cref="StartDate" /> onwards are published. A start date earlier than today is not supported.
/// </remarks>
public sealed class ProjectionOptions
{
    /// <summary>The first date published in the projection, which must not be earlier than today.</summary>
    public required DateOnly StartDate { get; init; }

    /// <summary>The number of days published, starting at <see cref="StartDate" />.</summary>
    public required int DaysForecast { get; init; }
}
