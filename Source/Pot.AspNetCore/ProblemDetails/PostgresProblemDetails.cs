namespace Pot.AspNetCore.ProblemDetails;

public class PostgresProblemDetails : ProblemDetailsExtension
{
    public required string SqlState { get; init; }
}
