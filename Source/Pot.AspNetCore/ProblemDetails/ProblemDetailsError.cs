namespace Pot.AspNetCore.ProblemDetails;

internal sealed class ProblemDetailsError
{
    public required string PropertyName { get; init; }
    public string? ErrorCode { get; init; }
    public required object? AttemptedValue { get; init; }
    public required string ErrorMessage { get; init; }
}
