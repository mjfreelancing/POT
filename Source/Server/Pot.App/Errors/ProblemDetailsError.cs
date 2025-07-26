namespace Pot.App.Errors;

public class ProblemDetailsError : ProblemDetailsBasicError
{
    public string PropertyName { get; init; } = string.Empty;
    public object? AttemptedValue { get; init; }

    public ProblemDetailsError(ProblemType problemType)
        : base(problemType)
    {
    }
}

public class ProblemDetailsBasicError : ProblemDetailsErrorBase
{
    // These properties are specific error details
    public required string ErrorCode { get; init; }
    public required string ErrorMessage { get; init; }
    public object? CustomState { get; init; } = null;

    public ProblemDetailsBasicError(ProblemType problemType)
        : base(problemType)
    {
    }
}
