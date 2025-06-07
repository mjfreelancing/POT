namespace Pot.App.Errors;

public class ProblemDetailsError : ProblemDetailsErrorBase
{
    // These properties are specific error details
    public required string ErrorCode { get; init; }
    public string PropertyName { get; init; } = string.Empty;
    public object? AttemptedValue { get; init; }
    public required string ErrorMessage { get; init; }
    public object? CustomState { get; init; } = null;

    // TODO: can we use CODE and DESCRIPTION instead of ERROR_CODE and ERROR_MESSAGE?

    public ProblemDetailsError(ProblemType problemType)
        : base(problemType)
    {
    }
}
