namespace Pot.App.Features.Projections.Models;

public sealed class ProjectionOptions
{
    public required DateOnly StartDate { get; init; }
    public required int DaysForecast { get; init; }
}
