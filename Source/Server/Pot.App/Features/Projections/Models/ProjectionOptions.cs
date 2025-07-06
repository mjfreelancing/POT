namespace Pot.App.Features.Projections.Models;

public sealed class ProjectionOptions
{
    public DateOnly StartDate { get; init; } = DateOnly.FromDateTime(TimeProvider.System.GetLocalNow().DateTime);
    public int DaysForecast { get; init; }
}
