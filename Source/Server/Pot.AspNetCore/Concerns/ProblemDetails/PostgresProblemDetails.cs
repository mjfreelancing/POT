namespace Pot.AspNetCore.Concerns.ProblemDetails;

internal sealed class PostgresProblemDetails : ProblemDetailsExtensionBase
{
    public required string SqlState { get; init; }
}
