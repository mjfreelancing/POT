namespace Pot.AspNetCore.Concerns.ProblemDetails;

internal class ProblemDetailsError
{
    public required string PropertyName { get; init; }
    public required string ErrorCode { get; init; }
    public required object? AttemptedValue { get; init; }
    public required string ErrorMessage { get; init; }
}
