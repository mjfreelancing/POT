namespace Pot.AspNetCore.ProblemDetails;

internal class ValidationProblemDetails : ProblemDetailsExtension
{
    public string? Property { get; init; }
    public string? ErrorCode { get; init; }
    public object? AttemptedValue { get; init; }
}
