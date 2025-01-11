namespace Pot.AspNetCore.ProblemDetails;

public class ValidationProblemDetails : ProblemDetailsExtension
{
    public string? Property { get; init; }
    public string? ErrorCode { get; init; }
    public object? AttemptedValue { get; init; }
}
