namespace Pot.AspNetCore.ProblemDetails;

internal class PostgresProblemDetails : ProblemDetailsExtension
{
    public required string SqlState { get; init; }
}
