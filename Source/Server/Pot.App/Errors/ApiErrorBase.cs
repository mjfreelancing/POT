using AllOverIt.Patterns.Result;

namespace Pot.App.Errors;

public abstract class ApiErrorBase : EnrichedError<ErrorType>
{
    protected ApiErrorBase(ErrorType errorType)
        : base(errorType)
    {
    }
}
