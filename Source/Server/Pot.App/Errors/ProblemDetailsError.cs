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
