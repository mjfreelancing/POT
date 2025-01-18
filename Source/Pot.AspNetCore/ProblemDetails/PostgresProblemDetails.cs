namespace Pot.AspNetCore.ProblemDetails;

internal sealed class PostgresProblemDetails : ProblemDetailsExtensionBase
{
    public required string SqlState { get; init; }
}
