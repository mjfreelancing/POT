using AllOverIt.Patterns.Result;

namespace Pot.App.Errors;

public abstract class ProblemDetailsErrorBase : EnrichedError<ProblemType>
{
    protected ProblemDetailsErrorBase(ProblemType errorType)
        : base(errorType)
    {
    }
}
