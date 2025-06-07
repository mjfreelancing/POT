namespace Pot.App.Errors;

public class ProblemDetailsErrorCollection : ProblemDetailsErrorBase
{
    public ProblemDetailsError[] Errors { get; init; }

    public ProblemDetailsErrorCollection(ProblemType problemType, IEnumerable<ProblemDetailsError> errors)
        : base(problemType)
    {
        Errors = [.. errors];
    }
}