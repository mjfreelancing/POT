using AllOverIt.Assertion;
using AllOverIt.Patterns.Result;

namespace Pot.AspNetCore.Errors;

public sealed class ServiceError : EnrichedError
{
    public Microsoft.AspNetCore.Mvc.ProblemDetails ProblemDetails { get; }

    public ServiceError(Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails)
    {
        ProblemDetails = problemDetails.WhenNotNull();
    }
}