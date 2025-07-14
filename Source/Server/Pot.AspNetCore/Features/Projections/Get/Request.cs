namespace Pot.AspNetCore.Features.Projections.Get;

internal sealed class Request
{
    public required DateOnly StartDate { get; init; }
    public required DateOnly EndDate { get; init; }
}
